using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HEVEQ.Application.Common.AI.Models.Search;

namespace HEVEQ.Application.Common.Persistence.Interfaces;

public interface IMarketplaceListingReadRepository
{
    Task<IReadOnlyList<MarketplaceListingSnapshot>> GetActiveSnapshotsByIdAsync(
        IEnumerable<Guid> listingIds,
        CancellationToken ct = default);
}
