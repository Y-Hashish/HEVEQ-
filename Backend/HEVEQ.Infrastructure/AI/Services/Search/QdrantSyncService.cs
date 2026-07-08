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

public sealed class QdrantSyncService
{
    private const int EmbeddingStatusSynced = 1;
    private const int EmbeddingStatusFailed = 2;
    private const ulong VectorDimension = 1536;

    private readonly IServiceListingReadRepository _listingRepository;
    private readonly IQdrantListingIndex _qdrantIndex;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly QdrantClient _qdrantClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QdrantSyncService> _logger;
    private readonly string _collectionName;
    private readonly string _marketplaceCollectionName;

    public QdrantSyncService(
        IServiceListingReadRepository listingRepository,
        IQdrantListingIndex qdrantIndex,
        ITextEmbeddingGenerationService embeddingService,
        QdrantClient qdrantClient,
        ApplicationDbContext context,
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
        _marketplaceCollectionName = configuration["Qdrant:MarketplaceCollectionName"] ?? "marketplace_listings";
    }

    public async Task<QdrantSyncResult> SyncAllActiveListingsAsync(CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        await EnsureCollectionExistsAsync(ct);

        var listings = await _listingRepository.GetAllActiveForEmbeddingAsync(ct);
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

                    await _qdrantIndex.UpsertAsync(
                        listing.Id,
                        vector,
                        listing.BuildQdrantPayload(),
                        ct);

                    syncedIds.Add(listing.Id);
                    synced++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    failed++;
                    failedIds.Add(listing.Id);
                    _logger.LogWarning(ex, "Failed to sync service listing {Id}.", listing.Id);
                    await MarkFailedAsync(listing.Id, ct);
                }
            }

            if (syncedIds.Count > 0)
                await MarkSyncedAsync(syncedIds, ct);

            if (!ct.IsCancellationRequested)
                await Task.Delay(300, ct);
        }

        return new QdrantSyncResult(listings.Count, synced, failed, failedIds, DateTime.UtcNow - startedAt);
    }

    public async Task<QdrantSyncResult> SyncAllActiveMarketplaceListingsAsync(CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        await EnsureCollectionExistsAsync(ct);

        var listings = await _context.MarketplaceListings
            .AsNoTracking()
            .Include(l => l.Category)
            .Include(l => l.Seller)
            .Where(l => l.Status == MarketplaceListingStatus.Active)
            .Select(l => new
            {
                l.Id,
                l.Title,
                l.Description,
                l.Specifications,
                l.CategoryId,
                CategoryName = l.Category.Name,
                l.Price,
                Condition = l.Condition.ToString(),
                l.YearOfManufacture,
                l.Governorate,
                l.District,
                SellerName = (l.Seller.FirstName + " " + l.Seller.LastName).Trim()
            })
            .ToListAsync(ct);

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
                        BuildMarketplaceEmbeddingText(
                            listing.CategoryName,
                            listing.Title,
                            listing.Description,
                            listing.Specifications,
                            listing.Condition,
                            listing.YearOfManufacture,
                            listing.Governorate,
                            listing.District,
                            listing.SellerName),
                        cancellationToken: ct);

                    await UpsertMarketplaceAsync(
                        listing.Id,
                        vector,
                        BuildMarketplacePayload(
                            listing.CategoryId,
                            listing.CategoryName,
                            listing.Price,
                            listing.Condition,
                            listing.Governorate,
                            listing.District,
                            listing.SellerName,
                            listing.Title),
                        ct);

                    syncedIds.Add(listing.Id);
                    synced++;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    failed++;
                    failedIds.Add(listing.Id);
                    _logger.LogWarning(ex, "Failed to sync marketplace listing {Id}.", listing.Id);
                    await MarkMarketplaceFailedAsync(listing.Id, ct);
                }
            }

            if (syncedIds.Count > 0)
                await MarkMarketplaceSyncedAsync(syncedIds, ct);

            if (!ct.IsCancellationRequested)
                await Task.Delay(300, ct);
        }

        return new QdrantSyncResult(listings.Count, synced, failed, failedIds, DateTime.UtcNow - startedAt);
    }

    public async Task<QdrantCollectionStatus> GetCollectionStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var collections = await _qdrantClient.ListCollectionsAsync(ct);
            if (!collections.Contains(_collectionName))
                return new QdrantCollectionStatus(_collectionName, false, null, "Not created");

            var info = await _qdrantClient.GetCollectionInfoAsync(_collectionName, ct);
            return new QdrantCollectionStatus(_collectionName, true, info.PointsCount, info.Status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query Qdrant collection status.");
            return new QdrantCollectionStatus(_collectionName, false, null, $"Error: {ex.Message}");
        }
    }

    private async Task EnsureCollectionExistsAsync(CancellationToken ct)
    {
        var collections = await _qdrantClient.ListCollectionsAsync(ct);

        if (!collections.Contains(_collectionName))
        {
            await _qdrantClient.CreateCollectionAsync(
                collectionName: _collectionName,
                vectorsConfig: new VectorParams { Size = VectorDimension, Distance = Distance.Cosine },
                cancellationToken: ct);

            await _qdrantClient.CreatePayloadIndexAsync(_collectionName, "is_active", PayloadSchemaType.Bool, cancellationToken: ct);
            await _qdrantClient.CreatePayloadIndexAsync(_collectionName, "category_id", PayloadSchemaType.Integer, cancellationToken: ct);
            await _qdrantClient.CreatePayloadIndexAsync(_collectionName, "provider_location", PayloadSchemaType.Geo, cancellationToken: ct);
            await _qdrantClient.CreatePayloadIndexAsync(_collectionName, "provider_profile_id", PayloadSchemaType.Keyword, cancellationToken: ct);
        }

        if (!collections.Contains(_marketplaceCollectionName))
        {
            await _qdrantClient.CreateCollectionAsync(
                collectionName: _marketplaceCollectionName,
                vectorsConfig: new VectorParams { Size = VectorDimension, Distance = Distance.Cosine },
                cancellationToken: ct);

            await _qdrantClient.CreatePayloadIndexAsync(_marketplaceCollectionName, "is_active", PayloadSchemaType.Bool, cancellationToken: ct);
            await _qdrantClient.CreatePayloadIndexAsync(_marketplaceCollectionName, "category_id", PayloadSchemaType.Integer, cancellationToken: ct);
        }
    }

    private async Task UpsertMarketplaceAsync(
        Guid listingId,
        ReadOnlyMemory<float> vector,
        IReadOnlyDictionary<string, object> payload,
        CancellationToken ct)
    {
        var qdrantVector = new Vector();
        qdrantVector.Data.AddRange(vector.Span.ToArray());

        var point = new PointStruct
        {
            Id = new PointId { Uuid = listingId.ToString() },
            Vectors = new Vectors { Vector = qdrantVector }
        };

        foreach (var (key, value) in payload)
            point.Payload[key] = ToValue(value);

        await _qdrantClient.UpsertAsync(
            collectionName: _marketplaceCollectionName,
            points: [point],
            cancellationToken: ct);
    }

    private static string BuildMarketplaceEmbeddingText(
        string categoryName,
        string title,
        string description,
        string? specifications,
        string condition,
        int? yearOfManufacture,
        string? governorate,
        string? district,
        string sellerName)
    {
        var parts = new List<string>
        {
            $"{categoryName} marketplace item",
            title,
            description,
            $"Condition: {condition}",
            $"Seller: {sellerName}"
        };

        if (!string.IsNullOrWhiteSpace(specifications)) parts.Add($"Specifications: {specifications}");
        if (yearOfManufacture.HasValue) parts.Add($"Year: {yearOfManufacture.Value}");
        if (!string.IsNullOrWhiteSpace(governorate)) parts.Add($"Governorate: {governorate}");
        if (!string.IsNullOrWhiteSpace(district)) parts.Add($"District: {district}");

        return string.Join(". ", parts);
    }

    private static IReadOnlyDictionary<string, object> BuildMarketplacePayload(
        int categoryId,
        string categoryName,
        decimal price,
        string condition,
        string? governorate,
        string? district,
        string sellerName,
        string title) =>
        new Dictionary<string, object>
        {
            ["is_active"] = true,
            ["category_id"] = categoryId,
            ["category_name"] = categoryName,
            ["price"] = (double)price,
            ["condition"] = condition,
            ["governorate"] = governorate ?? string.Empty,
            ["district"] = district ?? string.Empty,
            ["seller_name"] = sellerName,
            ["title"] = title
        };

    private static Value ToValue(object? obj) => obj switch
    {
        bool b => new Value { BoolValue = b },
        string s => new Value { StringValue = s },
        int i => new Value { IntegerValue = i },
        long l => new Value { IntegerValue = l },
        float f => new Value { DoubleValue = f },
        double d => new Value { DoubleValue = d },
        decimal m => new Value { DoubleValue = (double)m },
        Guid g => new Value { StringValue = g.ToString() },
        null => new Value { NullValue = NullValue.NullValue },
        _ => new Value { StringValue = obj.ToString() ?? string.Empty }
    };

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
                .SetProperty(sl => sl.EmbeddingStatus, (EmbeddingStatus)EmbeddingStatusFailed), ct);
    }

    private async Task MarkMarketplaceSyncedAsync(IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await _context.MarketplaceListings
            .Where(l => ids.Contains(l.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.EmbeddingStatus, (EmbeddingStatus)EmbeddingStatusSynced)
                .SetProperty(l => l.QdrantPointId, l => l.Id.ToString())
                .SetProperty(l => l.LastEmbeddedAt, now), ct);
    }

    private async Task MarkMarketplaceFailedAsync(Guid id, CancellationToken ct)
    {
        await _context.MarketplaceListings
            .Where(l => l.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.EmbeddingStatus, (EmbeddingStatus)EmbeddingStatusFailed), ct);
    }
}
