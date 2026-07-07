using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HEVEQ.Application.Common.AI.Models.Search;
using HEVEQ.Application.Common.Persistence.Interfaces;

namespace HEVEQ.Infrastructure.Persistence.Repositories;

public sealed class MarketplaceListingReadRepository : IMarketplaceListingReadRepository
{
    // A placeholder implementation returning an empty list.
    // Replace with actual database querying logic when DbContext is ready for marketplace listings.

    public Task<IReadOnlyList<MarketplaceListingSnapshot>> GetActiveSnapshotsByIdAsync(
        IEnumerable<Guid> listingIds,
        CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<MarketplaceListingSnapshot>>(new List<MarketplaceListingSnapshot>());
    }
}
