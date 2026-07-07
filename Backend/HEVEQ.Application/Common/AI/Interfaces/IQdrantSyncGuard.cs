using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces
{
    /// <summary>
    /// Qdrant synchronisation guard. Called whenever a ServiceListing is auto-blocked or suspended by the
    /// ModeratorAgent so the stale/toxic vector point is immediately excluded from similarity searches —
    /// without waiting for the DomainEventQueue background reconciler (GAP-008).
    /// Implemented in Infrastructure by QdrantSyncGuardService.
    /// </summary>
    public interface IQdrantSyncGuard
    {
        /// <summary>Marks the listing's Qdrant vector point as inactive (payload: is_active=false) and
        /// sets ServiceListings.EmbeddingStatus = 3 (Deleted).</summary>
        Task InvalidateListingVectorAsync(Guid serviceListingId, CancellationToken ct = default);

        /// <summary>Re-activates the vector point after an admin reinstates a listing and sets
        /// EmbeddingStatus back to 0 (Pending) so the background embedder re-syncs it cleanly.</summary>
        Task ReactivateListingVectorAsync(Guid serviceListingId, CancellationToken ct = default);
    }

}
