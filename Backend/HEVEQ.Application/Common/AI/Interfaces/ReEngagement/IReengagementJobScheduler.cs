using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.AI.Interfaces.ReEngagement
{
    /// <summary>
    /// Schedules re-engagement processing to run in the background approximately
    /// 30 seconds after a booking rejection, giving the booking state time to fully
    /// settle before the alternative-listing query executes.
    /// Infrastructure implementation: ChannelReengagementScheduler (backed by
    /// ReengagementBackgroundWorker : BackgroundService).
    /// </summary>
    public interface IReengagementJobScheduler
    {
        /// <summary>Enqueues rejectedBookingId for delayed processing.
        /// Fire-and-forget from the perspective of the MediatR notification handler.</summary>
        Task ScheduleAsync(Guid rejectedBookingId, CancellationToken ct = default);
    }
}
