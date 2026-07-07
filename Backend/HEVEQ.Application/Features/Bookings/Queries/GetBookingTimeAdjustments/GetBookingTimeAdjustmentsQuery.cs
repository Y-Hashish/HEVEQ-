using HEVEQ.Application.Features.Bookings.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Bookings.Queries.GetBookingTimeAdjustments
{
    public sealed record GetBookingTimeAdjustmentsQuery(Guid CustomerId, Guid BookingId) : IRequest<IReadOnlyList<CustomerTimeAdjustmentItemDto>>;
}