//using HEVEQ.Application.Common.Persistence.Interfaces;
//using HEVEQ.Domain.Entities; // عشان يشوف الـ EmbeddingStatus بتاعك
//using HEVEQ.Domain.Enums;
//using HEVEQ.Infrastructure.Persistence;
//using HEVEQ.Infrastructure.Persistence.Qdrant;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Microsoft.SemanticKernel.Embeddings;
//using Qdrant.Client;
//using Qdrant.Client.Grpc;

//namespace HEVEQ.Infrastructure.AI.Services.Search;

//public sealed record QdrantSyncResult(
//    int TotalListings,
//    int Synced,
//    int Failed,
//    IReadOnlyList<Guid> FailedIds,
//    TimeSpan Duration);

//public sealed record QdrantCollectionStatus(
//    string CollectionName,
//    bool Exists,
//    ulong? VectorCount,
//    string StatusText);

//public sealed class QdrantSyncService
//{
//    private const int EmbeddingStatusSynced = 1;
//    private const int EmbeddingStatusFailed = 2;
//    private const ulong VectorDimension = 1536;

//    private readonly IServiceListingReadRepository _listingRepository;
//    private readonly IQdrantListingIndex _qdrantIndex;
//    private readonly ITextEmbeddingGenerationService _embeddingService;
//    private readonly QdrantClient _qdrantClient;
//    private readonly ApplicationDbContext _context;
//    private readonly ILogger<QdrantSyncService> _logger;
//    private readonly string _collectionName;

//    public QdrantSyncService(
//        IServiceListingReadRepository listingRepository,
//        IQdrantListingIndex qdrantIndex,
//        ITextEmbeddingGenerationService embeddingService,
//        QdrantClient qdrantClient,
//        ApplicationDbContext context,
//        IConfiguration configuration,
//        ILogger<QdrantSyncService> logger)
//    {
//        _listingRepository = listingRepository;
//        _qdrantIndex = qdrantIndex;
//        _embeddingService = embeddingService;
//        _qdrantClient = qdrantClient;
//        _context = context;
//        _logger = logger;
//        _collectionName = configuration["Qdrant:CollectionName"] ?? "service_listings";
//    }

//    public async Task<QdrantSyncResult> SyncAllActiveListingsAsync(CancellationToken ct = default)
//    {
//        var startedAt = DateTime.UtcNow;

//        await EnsureCollectionExistsAsync(ct);

//        _logger.LogInformation("Fetching active listings from SQL Server for embedding sync...");
//        var listings = await _listingRepository.GetAllActiveForEmbeddingAsync(ct);
//        _logger.LogInformation("Found {Count} active listings to sync.", listings.Count);

//        if (listings.Count == 0)
//            return new QdrantSyncResult(0, 0, 0, [], TimeSpan.Zero);

//        int synced = 0, failed = 0;
//        var failedIds = new List<Guid>();
//        var syncedIds = new List<Guid>(25);

//        var batches = listings.Chunk(25);

//        foreach (var batch in batches)
//        {
//            syncedIds.Clear();

//            foreach (var listing in batch)
//            {
//                try
//                {
//                    ct.ThrowIfCancellationRequested();

//                    var embeddingText = listing.BuildEmbeddingText();
//                    var vector = await _embeddingService.GenerateEmbeddingAsync(
//                                            embeddingText, cancellationToken: ct);

//                    await _qdrantIndex.UpsertAsync(
//                        listing.Id,
//                        vector,
//                        listing.BuildQdrantPayload(),
//                        ct);

//                    syncedIds.Add(listing.Id);
//                    synced++;

//                    _logger.LogDebug("Synced listing {Id} — \"{Title}\"", listing.Id, listing.Title);
//                }
//                catch (OperationCanceledException)
//                {
//                    throw;
//                }
//                catch (Exception ex)
//                {
//                    failed++;
//                    failedIds.Add(listing.Id);
//                    _logger.LogWarning(ex, "Failed to embed/upsert listing {Id}. Marking as Failed.", listing.Id);

//                    await MarkFailedAsync(listing.Id, ct);
//                }
//            }

//            if (syncedIds.Count > 0)
//                await MarkSyncedAsync(syncedIds, ct);

//            if (!ct.IsCancellationRequested)
//                await Task.Delay(300, ct);
//        }

//        var duration = DateTime.UtcNow - startedAt;
//        return new QdrantSyncResult(listings.Count, synced, failed, failedIds, duration);
//    }

//    public async Task<QdrantCollectionStatus> GetCollectionStatusAsync(CancellationToken ct = default)
//    {
//        try
//        {
//            var collections = await _qdrantClient.ListCollectionsAsync(ct);
//            if (!collections.Contains(_collectionName))
//                return new QdrantCollectionStatus(_collectionName, false, null, "Not created");

//            var info = await _qdrantClient.GetCollectionInfoAsync(_collectionName, ct);


//            return new QdrantCollectionStatus(
//                _collectionName,
//                Exists: true,
//                VectorCount: info.PointsCount,
//                StatusText: info.Status.ToString());
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to query Qdrant collection status.");
//            return new QdrantCollectionStatus(_collectionName, false, null, $"Error: {ex.Message}");
//        }
//    }

//    private async Task EnsureCollectionExistsAsync(CancellationToken ct)
//    {
//        var collections = await _qdrantClient.ListCollectionsAsync(ct);
//        if (collections.Contains(_collectionName))
//            return;

//        _logger.LogInformation("Creating Qdrant collection '{Name}' (dim=1536, distance=Cosine).", _collectionName);


//        await _qdrantClient.CreateCollectionAsync(
//            collectionName: _collectionName,
//            vectorsConfig: new VectorParams
//            {
//                Size = VectorDimension,
//                Distance = Distance.Cosine
//            },
//            cancellationToken: ct);

//        await _qdrantClient.CreatePayloadIndexAsync(
//            collectionName: _collectionName,
//            fieldName: "is_active",
//            schemaType: PayloadSchemaType.Bool,
//            cancellationToken: ct);

//        await _qdrantClient.CreatePayloadIndexAsync(
//            collectionName: _collectionName,
//            fieldName: "category_id",
//            schemaType: PayloadSchemaType.Integer,
//            cancellationToken: ct);
//    }

//    private async Task MarkSyncedAsync(IReadOnlyList<Guid> ids, CancellationToken ct)
//    {
//        var now = DateTime.UtcNow;
//        await _context.ServiceListings
//            .Where(sl => ids.Contains(sl.Id))
//            .ExecuteUpdateAsync(s => s
//                .SetProperty(sl => sl.EmbeddingStatus, sl => (EmbeddingStatus)EmbeddingStatusSynced)
//                .SetProperty(sl => sl.QdrantPointId, sl => sl.Id.ToString())
//                .SetProperty(sl => sl.LastEmbeddedAt, now),
//                ct);
//    }

//    private async Task MarkFailedAsync(Guid listingId, CancellationToken ct)
//    {
//        await _context.ServiceListings
//            .Where(sl => sl.Id == listingId)
//            .ExecuteUpdateAsync(s => s
//                .SetProperty(sl => sl.EmbeddingStatus, sl => (EmbeddingStatus)EmbeddingStatusFailed),
//                ct);
//    }
//}




// ═══════════════════════════════════════════════════════════════════════════════
// TARGET PATH : Infrastructure/AI/Services/Search/QdrantSyncService.cs
// REPLACES    : previous QdrantSyncService.cs
// CHANGE      : EnsureCollectionExistsAsync now creates two extra payload indexes:
//                 • provider_location (Geo)     — used by QuerySimilarInRadiusAsync
//                 • provider_profile_id (Keyword) — used by MustNot exclusion filter
//               Without these, Qdrant does a full payload scan on every re-engagement
//               query instead of using the index.
//
// ADAPT: Replace "ApplicationDbContext" with your actual DbContext name.
// ═══════════════════════════════════════════════════════════════════════════════
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Domain.Enums;
using HEVEQ.Infrastructure.Persistence;
using HEVEQ.Infrastructure.Persistence.Qdrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HEVEQ.Infrastructure.AI.Services.Search;

// ─── Result DTOs ──────────────────────────────────────────────────────────────

public sealed record QdrantSyncResult(
    int TotalListings,
    int Synced,
    int Failed,
    IReadOnlyList<Guid> FailedIds,
    TimeSpan Duration);

public sealed record QdrantCollectionStatus(
    string CollectionName,
    bool Exists,
    ulong? VectorCount,
    string StatusText);

// ─── Service ──────────────────────────────────────────────────────────────────

public sealed class QdrantSyncService
{
    private const int EmbeddingStatusSynced = 1;
    private const int EmbeddingStatusFailed = 2;
    private const ulong VectorDimension = 1536; // text-embedding-3-small

    private readonly IServiceListingReadRepository _listingRepository;
    private readonly IQdrantListingIndex _qdrantIndex;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly QdrantClient _qdrantClient;
    private readonly ApplicationDbContext _context;      // ADAPT class name
    private readonly ILogger<QdrantSyncService> _logger;
    private readonly string _collectionName;

    public QdrantSyncService(
        IServiceListingReadRepository listingRepository,
        IQdrantListingIndex qdrantIndex,
        ITextEmbeddingGenerationService embeddingService,
        QdrantClient qdrantClient,
        ApplicationDbContext context,               // ADAPT class name
        IConfiguration configuration,
        ILogger<QdrantSyncService> logger)
    {
        _listingRepository = listingRepository;
        _qdrantIndex = qdrantIndex;
        _embeddingService = embeddingService;
        _qdrantClient = qdrantClient;
        _context = context;
        _logger = logger;
        _collectionName = configuration["Qdrant:CollectionName"] ?? "service_listings";
    }

    // ─── Full reindex ──────────────────────────────────────────────────────────

    public async Task<QdrantSyncResult> SyncAllActiveListingsAsync(CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        await EnsureCollectionExistsAsync(ct);

        _logger.LogInformation("Loading active listings from SQL Server...");
        var listings = await _listingRepository.GetAllActiveForEmbeddingAsync(ct);
        _logger.LogInformation("Found {Count} listings to sync.", listings.Count);

        if (listings.Count == 0)
            return new QdrantSyncResult(0, 0, 0, [], TimeSpan.Zero);

        int synced = 0, failed = 0;
        var failedIds = new List<Guid>();
        var syncedIds = new List<Guid>(25);

        foreach (var batch in listings.Chunk(25))
        {
            syncedIds.Clear();

            foreach (var listing in batch)
            {
                try
                {
                    ct.ThrowIfCancellationRequested();

                    var vector = await _embeddingService.GenerateEmbeddingAsync(
                        listing.BuildEmbeddingText(), cancellationToken: ct);

                    // BuildQdrantPayload now includes provider_location GeoPoint
                    await _qdrantIndex.UpsertAsync(
                        listing.Id, vector, listing.BuildQdrantPayload(), ct);

                    syncedIds.Add(listing.Id);
                    synced++;
                    _logger.LogDebug("Synced {Id} — \"{Title}\"", listing.Id, listing.Title);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    failed++;
                    failedIds.Add(listing.Id);
                    _logger.LogWarning(ex, "Failed to sync listing {Id}.", listing.Id);
                    await MarkFailedAsync(listing.Id, ct);
                }
            }

            if (syncedIds.Count > 0)
                await MarkSyncedAsync(syncedIds, ct);

            if (!ct.IsCancellationRequested)
                await Task.Delay(300, ct);
        }

        var duration = DateTime.UtcNow - startedAt;
        _logger.LogInformation(
            "Sync complete. Synced={S} Failed={F} Duration={D}", synced, failed, duration);

        return new QdrantSyncResult(listings.Count, synced, failed, failedIds, duration);
    }

    // ─── Collection status (GET /api/admin/search/status) ────────────────────

    public async Task<QdrantCollectionStatus> GetCollectionStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var collections = await _qdrantClient.ListCollectionsAsync(ct);
            if (!collections.Contains(_collectionName))
                return new QdrantCollectionStatus(_collectionName, false, null, "Not created");

            var info = await _qdrantClient.GetCollectionInfoAsync(_collectionName, ct);
            return new QdrantCollectionStatus(
                _collectionName, true, info.PointsCount, info.Status.ToString());
        }
        catch (Exception ex)
        {
            return new QdrantCollectionStatus(_collectionName, false, null, $"Error: {ex.Message}");
        }
    }

    // ─── Collection bootstrap (runs once, idempotent) ─────────────────────────

    private async Task EnsureCollectionExistsAsync(CancellationToken ct)
    {
        var collections = await _qdrantClient.ListCollectionsAsync(ct);
        if (collections.Contains(_collectionName))
        {
            _logger.LogDebug("Collection '{Name}' already exists.", _collectionName);
            return;
        }

        _logger.LogInformation("Creating collection '{Name}' (dim={D}, Cosine).",
            _collectionName, VectorDimension);

        await _qdrantClient.CreateCollectionAsync(
            collectionName: _collectionName,
            vectorsConfig: 
                new VectorParams
                {
                    Size = VectorDimension,
                    Distance = Distance.Cosine
                }
            ,
            cancellationToken: ct);

        // ── Payload indexes ───────────────────────────────────────────────────
        // 1. is_active (Bool) — primary filter on ALL searches
        await _qdrantClient.CreatePayloadIndexAsync(
            _collectionName, "is_active",
            PayloadSchemaType.Bool, cancellationToken: ct);

        // 2. category_id (Integer) — category equality filter in re-engagement
        await _qdrantClient.CreatePayloadIndexAsync(
            _collectionName, "category_id",
            PayloadSchemaType.Integer, cancellationToken: ct);

        // 3. provider_location (Geo) — geo-radius filter in QuerySimilarInRadiusAsync
        //    Without this index Qdrant performs a full payload scan for every
        //    Task 3 re-engagement query.
        await _qdrantClient.CreatePayloadIndexAsync(
            _collectionName, "provider_location",
            PayloadSchemaType.Geo, cancellationToken: ct);

        // 4. provider_profile_id (Keyword) — MustNot exclusion in re-engagement
        await _qdrantClient.CreatePayloadIndexAsync(
            _collectionName, "provider_profile_id",
            PayloadSchemaType.Keyword, cancellationToken: ct);

        _logger.LogInformation("Collection '{Name}' created with 4 payload indexes.", _collectionName);
    }

    // ─── DB write-back helpers ────────────────────────────────────────────────

    private async Task MarkSyncedAsync(IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await _context.ServiceListings
            .Where(sl => ids.Contains(sl.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(sl => sl.EmbeddingStatus, (EmbeddingStatus)EmbeddingStatusSynced)
                .SetProperty(sl => sl.QdrantPointId, sl => sl.Id.ToString())
                .SetProperty(sl => sl.LastEmbeddedAt, now), ct);
    }

    private async Task MarkFailedAsync(Guid id, CancellationToken ct)
    {
        await _context.ServiceListings
            .Where(sl => sl.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(sl => sl.EmbeddingStatus,(EmbeddingStatus) EmbeddingStatusFailed), ct);
    }
}