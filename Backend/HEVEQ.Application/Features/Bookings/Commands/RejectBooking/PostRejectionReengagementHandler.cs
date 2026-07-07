using HEVEQ.Application.Common.AI.Interfaces.ReEngagement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Bookings.Commands.RejectBooking
{
    public sealed class PostRejectionReengagementHandler
        : INotificationHandler<BookingRejectedNotification>
    {
        private readonly IReengagementJobScheduler _scheduler;

        public PostRejectionReengagementHandler(IReengagementJobScheduler scheduler)
            => _scheduler = scheduler;

        public Task Handle(
            BookingRejectedNotification notification,
            CancellationToken cancellationToken) =>
            _scheduler.ScheduleAsync(notification.BookingId, cancellationToken);
    }
}
