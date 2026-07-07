using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HEVEQ.Application.Common.AI.Models.Search;

namespace HEVEQ.Application.Common.AI.Interfaces.Search;

public interface IMarketplaceVectorSearchService
{
    Task<IReadOnlyList<VectorSearchHit>> SearchAsync(
        SearchIntent intent,
        int topK = 15,
        CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, string>> ExplainAllMatchesAsync(
        SearchIntent intent,
        IReadOnlyList<MarketplaceListingSnapshot> listings,
        CancellationToken ct = default);
}
