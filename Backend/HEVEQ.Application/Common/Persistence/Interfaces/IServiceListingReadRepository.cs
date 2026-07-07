using HEVEQ.Application.Common.AI.Models.Search;

namespace HEVEQ.Application.Common.Persistence.Interfaces;

public interface IServiceListingReadRepository
{
    // ── Used by SearchServicesQueryHandler (vector-search hydration) ──────────

    /// <summary>
    /// Loads SQL Server projections for a set of listing IDs returned by Qdrant,
    /// filtering strictly to Status = 2 (Active). Listings not found or no longer
    /// active are silently omitted — the caller must not assume the returned count
    /// equals the input count. This is the second safety net after Qdrant's own
    /// is_active payload filter.
    /// </summary>
    Task<IReadOnlyList<ServiceListingSnapshot>> GetActiveSnapshotsByIdAsync(
        IReadOnlyCollection<Guid> serviceListingIds,
        CancellationToken ct = default);

    // ── Used by PostRejectionAlternativesService (re-engagement flow) ─────────

    /// <summary>
    /// Finds up to <paramref name="maxResults"/> alternative active listings
    /// in the same category, excluding the provider that rejected the booking,
    /// ordered by Haversine distance from the booking's site coordinates.
    /// </summary>
    Task<IReadOnlyList<ServiceListingSnapshot>> FindAlternativesAsync(
        int categoryId,
        decimal? maxHourlyRate,
        double? originLatitude,
        double? originLongitude,
        Guid excludeProviderProfileId,
        int maxResults,
        CancellationToken ct = default);

    // ── Used by QdrantSyncService (initial / full reindex) ────────────────────

    /// <summary>
    /// Fetches every active ServiceListing with its Category and ProviderProfile
    /// data required to build the embedding text and Qdrant payload.
    /// Called only during full reindex operations — not on the hot search path.
    /// </summary>
    Task<IReadOnlyList<ServiceListingForEmbedding>> GetAllActiveForEmbeddingAsync(
        CancellationToken ct = default);
}