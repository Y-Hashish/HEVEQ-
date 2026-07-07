using HEVEQ.Application.Common.AI.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.Search
{
    /// <summary>
    /// Embeds the resolved search intent and queries Qdrant, then synthesises RAG
    /// match explanations for all hydrated candidates in a SINGLE LLM turn (batch),
    /// eliminating the N-per-listing API call bottleneck.
    /// Infrastructure implementation: QdrantServiceVectorSearchService
    /// </summary>
    public interface IServiceVectorSearchService
    {
        /// <summary>Embeds {EquipmentType} + {TaskDescription}, runs Qdrant top-K query,
        /// returns hits ordered by similarity score descending.</summary>
        Task<IReadOnlyList<VectorSearchHit>> SearchAsync(
            SearchIntent intent,
            int topK = 15,
            CancellationToken ct = default);

        /// <summary>
        /// Sends all hydrated listings to the LLM in ONE batched
        /// prompt and returns a dictionary keyed by ServiceListingSnapshot.Id.
        /// The handler must treat a missing dictionary key as an empty explanation string
        /// and must never throw on the absence of a key.
        /// </summary>
        Task<IReadOnlyDictionary<Guid, string>> ExplainAllMatchesAsync(
            SearchIntent intent,
            IReadOnlyList<ServiceListingSnapshot> listings,
            CancellationToken ct = default);
    }
}
