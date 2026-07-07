using HEVEQ.Application.Common.AI.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.Persistence.Qdrant
{
    public interface IQdrantListingIndex
    {
        Task<IReadOnlyList<VectorSearchHit>> QuerySimilarAsync(
            ReadOnlyMemory<float> vector,
            int topK,
            CancellationToken ct = default);

        Task UpsertAsync(
            Guid serviceListingId,
            ReadOnlyMemory<float> vector,
            IReadOnlyDictionary<string, object> payload,
            CancellationToken ct = default);

        Task SetPayloadAsync(
            string qdrantPointId,
            IReadOnlyDictionary<string, object> payload,
            CancellationToken ct = default);
    }
}
