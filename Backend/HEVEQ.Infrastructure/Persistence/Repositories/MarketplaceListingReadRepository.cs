using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Domain.Enums;
using HEVEQ.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Infrastructure.Persistence.Repositories;

public sealed class MarketplaceListingReadRepository(ApplicationDbContext context) : IMarketplaceListingReadRepository
{
    public async Task<IReadOnlyList<MarketplaceListingSnapshot>> GetActiveSnapshotsByIdAsync(
        IEnumerable<Guid> listingIds,
        CancellationToken ct = default)
    {
        var ids = listingIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        return await context.MarketplaceListings
            .AsNoTracking()
            .Where(l => ids.Contains(l.Id) && l.Status == MarketplaceListingStatus.Active)
            .Include(l => l.Seller)
            .Select(l => new MarketplaceListingSnapshot(
                l.Id,
                l.SellerId,
                l.Title,
                l.Description,
                l.CategoryId,
                l.Price,
                l.Condition.ToString(),
                (l.Seller.FirstName + " " + l.Seller.LastName).Trim(),
                0,
                null))
            .ToListAsync(ct);
    }
}
