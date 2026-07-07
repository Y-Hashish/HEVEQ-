using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Features.Bookings.Commands.RejectBooking
{
    /// <summary>
    /// Published by the existing RejectBookingCommandHandler after the booking record
    /// commits (Status = 8 / Rejected, ProviderRejectionReason populated).
    /// </summary>
    public sealed record BookingRejectedNotification(Guid BookingId) : INotification;
}
