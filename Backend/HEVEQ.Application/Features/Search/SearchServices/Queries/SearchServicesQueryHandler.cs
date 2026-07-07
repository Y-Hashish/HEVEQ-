using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Enums;
using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Application.Common.Persistence.Models;
using HEVEQ.Application.Common.Persistence.Enums;
using MediatR;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace HEVEQ.Application.Features.Search.SearchServices.Queries
{
    public sealed class SearchServicesQueryHandler
    : IRequestHandler<SearchServicesQuery, SearchServicesResult>
    {
        private readonly ISearchIntentExtractor _intentExtractor;
        private readonly IGeofenceValidator _geofenceValidator;
        private readonly IClarificationService _clarificationService;
        private readonly IServiceVectorSearchService _vectorSearch;
        private readonly IServiceListingReadRepository _listingRepository;
        private readonly IMarketplaceVectorSearchService _marketplaceVectorSearch;
        private readonly IMarketplaceListingReadRepository _marketplaceListingRepository;
        private readonly ISearchQueryLogWriter _searchLogWriter;

        public SearchServicesQueryHandler(
            ISearchIntentExtractor intentExtractor,
            IGeofenceValidator geofenceValidator,
            IClarificationService clarificationService,
            IServiceVectorSearchService vectorSearch,
            IServiceListingReadRepository listingRepository,
                  IMarketplaceVectorSearchService marketplaceVectorSearch,
            IMarketplaceListingReadRepository marketplaceListingRepository,
            ISearchQueryLogWriter searchLogWriter)
        {
            _intentExtractor = intentExtractor;
            _geofenceValidator = geofenceValidator;
            _clarificationService = clarificationService;
            _vectorSearch = vectorSearch;
            _listingRepository = listingRepository;
            _marketplaceVectorSearch = marketplaceVectorSearch;
            _marketplaceListingRepository = marketplaceListingRepository;
            _searchLogWriter = searchLogWriter;
        }

        public async Task<SearchServicesResult> Handle(
            SearchServicesQuery request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            // ── Step 1: Intent extraction ─────────────────────────────────────────
            var intent = await _intentExtractor.ExtractIntentAsync(
                request.RawQuery,
                request.ConversationHistory,
                cancellationToken);

            // ── Step 2: Null-parameter gate 
       
            var missingParams = new List<MissingSearchParameter>(intent.GetMissingParameters());

          
            if (intent.Target == SearchTarget.Ambiguous)
            {
                missingParams.Add(MissingSearchParameter.SearchTarget);
            }

            if (intent.Target == SearchTarget.Marketplace)
            {
                missingParams.Remove(MissingSearchParameter.Location);
            }

            if (missingParams.Count > 0)
                return await ReturnClarificationAsync(intent, missingParams, request, sw, cancellationToken);

            // ── Step 3: Geofencing gate (تعديل الـ Validation للـ Marketplace) ──


            if (intent.Target == SearchTarget.Marketplace && string.IsNullOrWhiteSpace(intent.Location))
            {
                // Skip geofencing validation, location stays null/empty safely
            }
            else
            {
       
                var geo = _geofenceValidator.Validate(intent.Location);

                if (!geo.IsValid)
                {
          
                    if (intent.Target == SearchTarget.ServiceListing)
                    {
                        missingParams.Add(MissingSearchParameter.InvalidLocation);
                        return await ReturnClarificationAsync(intent, missingParams, request, sw, cancellationToken);
                    }
                    
                    else if (intent.Target == SearchTarget.Marketplace)
                    {
                        intent = intent with { Location = null };
                    }
                }
                else
                {
                  
                    intent = intent with { Location = geo.NormalizedGovernorate };
                }
            }

            // ── Step 4: Qdrant vector search ──────────────────────────────────────
            //var hits = await _vectorSearch.SearchAsync(intent, topK: 15, cancellationToken);
            var results = new List<SearchResultItem>();

            // ── Step 5: SQL Server hydration — Active listings only ───────────────
            if (intent.Target == SearchTarget.Marketplace)
            {
                var hits = await _marketplaceVectorSearch.SearchAsync(intent, topK: 15, cancellationToken);
                var listingIds = hits.Select(h => h.ServiceListingId).ToList();
                //var activeListings = await _listingRepository.GetActiveSnapshotsByIdAsync(
                //    listingIds, cancellationToken);

                var activeListings = await _marketplaceListingRepository.GetActiveSnapshotsByIdAsync(listingIds, cancellationToken);

                var scoreById = hits.ToDictionary(h => h.ServiceListingId, h => h.SimilarityScore);
            var orderedActive = activeListings
                .OrderByDescending(l => scoreById.GetValueOrDefault(l.Id))
                .Take(10)
                .ToList();

            // ── Step 6: Batch RAG explanation ─────────────────────────────────────
            IReadOnlyDictionary<Guid, string> explanations = new Dictionary<Guid, string>();

            if (orderedActive.Count > 0)
            {
                    //explanations = await _vectorSearch.ExplainAllMatchesAsync(
                    //    intent, orderedActive, cancellationToken);

                    explanations = await _marketplaceVectorSearch.ExplainAllMatchesAsync(intent, orderedActive, cancellationToken);
                }

                results = orderedActive
                         .Select(l => new SearchResultItem(
                             null,
                             l,
                             scoreById.GetValueOrDefault(l.Id),
                             explanations.TryGetValue(l.Id, out var ex) ? ex : string.Empty))
                         .ToList();
            }
            else
            {
                var hits = await _vectorSearch.SearchAsync(intent, topK: 15, cancellationToken);
                var listingIds = hits.Select(h => h.ServiceListingId).ToList();
                var activeListings = await _listingRepository.GetActiveSnapshotsByIdAsync(listingIds, cancellationToken);

                var scoreById = hits.ToDictionary(h => h.ServiceListingId, h => h.SimilarityScore);
                var orderedActive = activeListings
                    .OrderByDescending(l => scoreById.GetValueOrDefault(l.Id))
                    .Take(10)
                    .ToList();

                IReadOnlyDictionary<Guid, string> explanations = new Dictionary<Guid, string>();
                if (orderedActive.Count > 0)
                {
                    explanations = await _vectorSearch.ExplainAllMatchesAsync(intent, orderedActive, cancellationToken);
                }

                results = orderedActive
                    .Select(l => new SearchResultItem(
                        l,
                        null,
                        scoreById.GetValueOrDefault(l.Id),
                        explanations.TryGetValue(l.Id, out var ex) ? ex : string.Empty))
                    .ToList();
            }

            sw.Stop();

            // ── Step 7: Analytics log ─────────────────────────────────────────────
            await WriteLogAsync(request, intent, results.Count, sw.ElapsedMilliseconds, cancellationToken);

            return SearchServicesResult.ReadyWithResults(intent, results, sw.ElapsedMilliseconds);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private async Task<SearchServicesResult> ReturnClarificationAsync(
            SearchIntent intent,
            IReadOnlyList<MissingSearchParameter> missingParams,
            SearchServicesQuery request,
            Stopwatch sw,
            CancellationToken ct)
        {
            var clarification = await _clarificationService
                .BuildClarificationAsync(intent, missingParams, ct);

            sw.Stop();

            await WriteLogAsync(request, intent, resultCount: 0, sw.ElapsedMilliseconds, ct);

            return SearchServicesResult.NeedsClarification(intent, clarification, sw.ElapsedMilliseconds);
        }

        private Task WriteLogAsync(
                    SearchServicesQuery request,
                    SearchIntent intent,
                    int resultCount,
                    long elapsedMs,
                    CancellationToken ct) =>
                    _searchLogWriter.LogAsync(new SearchQueryLogEntry(
                        UserId: request.RequestingUserId,
                        SessionId: request.SessionId,
                        RawQuery: request.RawQuery,
                        ExtractedIntentJson: JsonSerializer.Serialize(intent),
                       
                        ContextDomain: intent.Target == SearchTarget.Marketplace ? SearchContextDomain.Marketplace : SearchContextDomain.Services,
                        SearchMode: SearchMode.Semantic,
                        ResultCount: resultCount,
                        HasZeroResults: resultCount == 0,
                        ProcessingMs: elapsedMs), ct);
    }
}
