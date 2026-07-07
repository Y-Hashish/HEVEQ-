using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Infrastructure.AI.Plugins;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HEVEQ.Infrastructure.AI.Services.Search;

public sealed class QdrantMarketplaceVectorSearchService : IMarketplaceVectorSearchService
{
    private readonly SearchServicesPlugin _plugin;

    public QdrantMarketplaceVectorSearchService(SearchServicesPlugin plugin)
        => _plugin = plugin;

    public Task<IReadOnlyList<VectorSearchHit>> SearchAsync(
        SearchIntent intent,
        int topK = 15,
        CancellationToken ct = default) =>
        _plugin.SearchSimilarListingsAsync( // For now reuse SearchSimilarListingsAsync which just hits qdrant
            intentText: $"{intent.EquipmentType} {intent.TaskDescription}",
            topK: topK,
            ct: ct);

    public Task<IReadOnlyDictionary<Guid, string>> ExplainAllMatchesAsync(
        SearchIntent intent,
        IReadOnlyList<MarketplaceListingSnapshot> listings,
        CancellationToken ct = default) =>
        _plugin.ExplainMarketplaceMatchesAsync(
            intent: new SearchIntentResponseDto(
                intent.Target.ToString(),
                intent.EquipmentType,
                intent.Location,
                intent.TaskDescription,
                intent.Language.ToString()),
            listings: listings,
            ct: ct);
}
