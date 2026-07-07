using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Infrastructure.Persistence.Repositories;

public sealed class ServiceListingReadRepository : IServiceListingReadRepository
{
   
    private const int StatusActive = 2;

  
    private readonly ApplicationDbContext _context;

    public ServiceListingReadRepository(ApplicationDbContext context)
        => _context = context;

    // ════════════════════════════════════════════════════════════════════════
    // 1. Vector-search hydration
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Hydrates Qdrant hit IDs into full listing snapshots. Uses a single SQL
    /// IN query; EF Core translates Contains() on a HashSet into an efficient
    /// IN clause. Only Status = 2 (Active) records are returned — stale Qdrant
    /// points whose listings were later suspended are silently dropped here.
    /// </summary>
    public async Task<IReadOnlyList<ServiceListingSnapshot>> GetActiveSnapshotsByIdAsync(
        IReadOnlyCollection<Guid> serviceListingIds,
        CancellationToken ct = default)
    {
        if (serviceListingIds.Count == 0)
            return Array.Empty<ServiceListingSnapshot>();

        // HashSet for O(1) Contains inside the EF Core expression tree
        var idSet = serviceListingIds.ToHashSet();

        return await _context.ServiceListings
            .AsNoTracking()
            .Where(sl => idSet.Contains(sl.Id) && (int)sl.Status == StatusActive)
            .Select(sl => new ServiceListingSnapshot(
                sl.Id,
                sl.ProviderProfileId,
                sl.Title,
                sl.Description,
                sl.CategoryId,
                sl.HourlyRate ??0.0m,
                sl.DailyRate,
                sl.ProviderProfile.CompanyName,    
                sl.ProviderProfile.AverageRating,
                null))                            
            .ToListAsync(ct);
    }

    // ════════════════════════════════════════════════════════════════════════
    // 2. Re-engagement alternative listings
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads candidate alternatives from SQL Server (same category, active, not
    /// the rejecting provider, optional price cap), then sorts them by Haversine
    /// distance from the booking's site coordinates in application memory.
    /// Fetches maxResults × 5 candidates from SQL so the in-memory sort has
    /// enough pool to pick the closest maxResults even when many are far away.
    /// </summary>
    public async Task<IReadOnlyList<ServiceListingSnapshot>> FindAlternativesAsync(
        int categoryId,
        decimal? maxHourlyRate,
        double? originLat,
        double? originLon,
        Guid excludeProviderProfileId,
        int maxResults,
        CancellationToken ct = default)
    {
        var query = _context.ServiceListings
            .AsNoTracking()
            .Where(sl => sl.CategoryId == categoryId
                      && (int)sl.Status == StatusActive
                      && sl.ProviderProfileId != excludeProviderProfileId);

        if (maxHourlyRate.HasValue)
            query = query.Where(sl => sl.HourlyRate <= maxHourlyRate.Value);

        // Pull a generous pool and sort by distance in memory
        var candidates = await query
            .OrderByDescending(sl => sl.ProviderProfile.AverageRating) // best-rated first
            .Take(maxResults * 5)
            .Select(sl => new
            {
                sl.Id,
                sl.ProviderProfileId,
                sl.Title,
                sl.Description,
                sl.CategoryId,
                sl.HourlyRate,
                sl.DailyRate,
                CompanyName = sl.ProviderProfile.CompanyName,
                AverageRating = sl.ProviderProfile.AverageRating,
                BaseLat = (double)sl.ProviderProfile.BaseLatitude,
                BaseLon = (double)sl.ProviderProfile.BaseLongitude
            })
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return Array.Empty<ServiceListingSnapshot>();

        // If caller supplied coordinates, sort by provider base distance;
        // otherwise fall through with rating-ordered results from SQL.
        if (originLat.HasValue && originLon.HasValue)
        {
            candidates = [.. candidates
                .OrderBy(c => Haversine(originLat.Value, originLon.Value, c.BaseLat, c.BaseLon))];
        }

        return candidates
            .Take(maxResults)
            .Select(c => new ServiceListingSnapshot(
                c.Id,
                c.ProviderProfileId,
                c.Title,
                c.Description,
                c.CategoryId,
                c.HourlyRate??0.0m,
                c.DailyRate,
                c.CompanyName,
                c.AverageRating,
                originLat.HasValue && originLon.HasValue
                    ? Haversine(originLat.Value, originLon.Value, c.BaseLat, c.BaseLon)
                    : null))
            .ToList();
    }

    // ════════════════════════════════════════════════════════════════════════
    // 3. Full reindex — all active listings with embedding fields
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Single query that fetches every Active listing joined with its Category
    /// (for the category name in the embedding text) and ProviderProfile (for
    /// company name and coordinates stored in the Qdrant payload).
    /// EF Core generates one LEFT JOIN per navigation property used in the Select.
    /// </summary>
    public async Task<IReadOnlyList<ServiceListingForEmbedding>> GetAllActiveForEmbeddingAsync(
        CancellationToken ct = default)
    {
        return await _context.ServiceListings
            .AsNoTracking()
            .Where(sl => (int)sl.Status == StatusActive)
            .Select(sl => new ServiceListingForEmbedding(
                sl.Id,
                sl.Title,
                sl.Description ?? string.Empty,
                sl.Tags,
                sl.EquipmentModel,
                sl.EquipmentCapacity,
                sl.CategoryId,
                sl.Category.Name,                          // nav property
                sl.HourlyRate??0.0m,
                sl.ProviderProfile.CompanyName,            // nav property
                sl.ProviderProfileId,
                (double)sl.ProviderProfile.BaseLatitude,
                (double)sl.ProviderProfile.BaseLongitude))
            .ToListAsync(ct);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Haversine great-circle distance (km)
    // ════════════════════════════════════════════════════════════════════════

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                          + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                          * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;
}