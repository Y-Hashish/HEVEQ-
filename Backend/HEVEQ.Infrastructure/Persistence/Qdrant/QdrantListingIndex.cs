using HEVEQ.Application.Common.AI.Models.Search;
using Microsoft.Extensions.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.Persistence.Qdrant;

/// <summary>
/// Concrete implementation of IQdrantListingIndex backed by a real Qdrant server.
/// Registered as Singleton — QdrantClient is thread-safe and connection-pooled.
///
/// CRITICAL: QuerySimilarAsync always applies the is_active payload filter so
/// suspended / auto-blocked listings are excluded at the vector-DB layer.
/// </summary>
public sealed class QdrantListingIndex : IQdrantListingIndex
{
    private readonly QdrantClient _client;
    private readonly string _collectionName;

    // Applied on every search — only points whose payload has is_active=true are returned.
    private static readonly Filter ActiveFilter = new()
    {
        Must =
        {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key   = "is_active",
                    Match = new Match { Boolean = true }
                }
            }
        }
    };

    public QdrantListingIndex(QdrantClient client, IConfiguration configuration)
    {
        _client = client;
        _collectionName = configuration["Qdrant:CollectionName"] ?? "service_listings";
    }

    // ── Similarity search ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<VectorSearchHit>> QuerySimilarAsync(
        ReadOnlyMemory<float> vector,
        int topK,
        CancellationToken ct = default)
    {
        var results = await _client.SearchAsync(
            collectionName: _collectionName,
            vector: vector,
            filter: ActiveFilter,
            limit: (ulong)topK,
            cancellationToken: ct);

        var hits = new List<VectorSearchHit>(results.Count);
        foreach (var point in results)
        {
            // Points are stored with UUID = ServiceListing.Id.ToString()
            if (!point.Id.HasUuid) continue;
            if (!Guid.TryParse(point.Id.Uuid, out var listingId)) continue;

            hits.Add(new VectorSearchHit(listingId, point.Score, point.Id.Uuid));
        }
        return hits;
    }

    // ── Upsert (called by the background DomainEventQueue embedder) ────────────

    public async Task UpsertAsync(
        Guid serviceListingId,
        ReadOnlyMemory<float> vector,
        IReadOnlyDictionary<string, object> payload,
        CancellationToken ct = default)
    {
        // Build the Qdrant Vector object from the float span
        var qdrantVector = new Vector();
        qdrantVector.Data.AddRange(vector.Span.ToArray());

        var point = new PointStruct
        {
            Id = new PointId { Uuid = serviceListingId.ToString() },
            Vectors = new Vectors { Vector = qdrantVector }
        };

        foreach (var (key, value) in payload)
            point.Payload[key] = ToValue(value);

        await _client.UpsertAsync(
            collectionName: _collectionName,
            points: [point],
            cancellationToken: ct);
    }

    // ── Payload update (used by QdrantSyncGuardService for is_active toggling) ──

    public async Task SetPayloadAsync(
        string qdrantPointId,
        IReadOnlyDictionary<string, object> payload,
        CancellationToken ct = default)
    {
        var qdrantPayload = payload.ToDictionary(
            kvp => kvp.Key,
            kvp => ToValue(kvp.Value));

        await _client.SetPayloadAsync(
            collectionName: _collectionName,
            payload: qdrantPayload,
            ids: new[] { Guid.Parse(qdrantPointId) },
            cancellationToken: ct);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a CLR object to the Qdrant protobuf Value type.
    /// Extend the switch arms if you store additional payload types.
    /// </summary>
    private static Value ToValue(object? obj) => obj switch
    {
        bool b => new Value { BoolValue = b },
        string s => new Value { StringValue = s },
        int i => new Value { IntegerValue = i },
        long l => new Value { IntegerValue = l },
        float f => new Value { DoubleValue = f },
        double d => new Value { DoubleValue = d },
        Guid g => new Value { StringValue = g.ToString() },
        null => new Value { NullValue = NullValue.NullValue },
        _ => new Value { StringValue = obj.ToString() ?? string.Empty }
    };
}
