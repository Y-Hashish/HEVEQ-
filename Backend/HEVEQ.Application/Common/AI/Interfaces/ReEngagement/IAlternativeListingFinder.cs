
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.AI.Models.ReEngagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.ReEngagement
{
    /// <summary>
    /// Queries the database for up to N nearby alternative active ServiceListings
    /// that match the rejected booking's category and customer site, then synthesises
    /// a warm personalised notification message presenting those alternatives.
    /// Infrastructure implementation: PostRejectionAlternativesService
    /// </summary>
    public interface IAlternativeListingFinder
    {
        Task<ReengagementNotificationResult> BuildReengagementAsync(
            Guid rejectedBookingId,
            int maxResults = 3,
            CancellationToken ct = default);
    }
}
