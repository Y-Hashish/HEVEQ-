using System;

namespace HEVEQ.Application.Common.AI.Models.Search;

/// <summary>A single result from a Qdrant similarity query.</summary>
public sealed record VectorSearchHit(
    Guid ServiceListingId,
    float SimilarityScore,
    string QdrantPointId);
