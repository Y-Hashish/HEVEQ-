using HEVEQ.Application.Common.AI.Models.Search;
using System;
using System.Collections.Generic;


namespace HEVEQ.Application.Common.AI.Models.ReEngagement;

/// <summary>
/// Output of the post-rejection re-engagement flow: up to three nearby alternative
/// active listings and the AI-synthesised notification message that will be sent to
/// the customer.
/// </summary>
public sealed record ReengagementNotificationResult(
    Guid RejectedBookingId,
    IReadOnlyList<ServiceListingSnapshot> SuggestedAlternatives,
    string NotificationMessage);