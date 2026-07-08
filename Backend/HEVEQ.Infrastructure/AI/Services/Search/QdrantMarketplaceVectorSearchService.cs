using HEVEQ.Application.Common.AI;
using HEVEQ.Application.Common.AI.Interfaces.Search;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Infrastructure.AI.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace HEVEQ.Infrastructure.AI.Services.Search;

public sealed class QdrantMarketplaceVectorSearchService : IMarketplaceVectorSearchService
{
    private readonly SearchServicesPlugin _plugin;
    private readonly ITextEmbeddingGenerationService _embedding;
    private readonly QdrantClient _client;
    private readonly string _collectionName;

    private static readonly Filter ActiveFilter = new()
    {
        Must =
        {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "is_active",
                    Match = new Match { Boolean = true }
                }
            }
        }
    };

    public QdrantMarketplaceVectorSearchService(
        SearchServicesPlugin plugin,
        ITextEmbeddingGenerationService embedding,
        QdrantClient client,
        IConfiguration configuration)
    {
        _plugin = plugin;
        _embedding = embedding;
        _client = client;
        _collectionName = configuration["Qdrant:MarketplaceCollectionName"] ?? "marketplace_listings";
    }

    public async Task<IReadOnlyList<VectorSearchHit>> SearchAsync(
        SearchIntent intent,
        int topK = 15,
        CancellationToken ct = default)
    {
        var vector = await _embedding.GenerateEmbeddingAsync(
            $"{intent.EquipmentType} {intent.TaskDescription}",
            cancellationToken: ct);

        var results = await _client.SearchAsync(
            collectionName: _collectionName,
            vector: vector,
            filter: ActiveFilter,
            limit: (ulong)topK,
            cancellationToken: ct);

        var hits = new List<VectorSearchHit>(results.Count);
        foreach (var point in results)
        {
            if (!point.Id.HasUuid) continue;
            if (!Guid.TryParse(point.Id.Uuid, out var listingId)) continue;
            hits.Add(new VectorSearchHit(listingId, point.Score, point.Id.Uuid));
        }

        return hits;
    }

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
