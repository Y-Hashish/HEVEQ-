using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Bookings.Queries.GetBookingTimeAdjustments
{
    public sealed class GetBookingTimeAdjustmentsQueryHandler : IRequestHandler<GetBookingTimeAdjustmentsQuery, IReadOnlyList<CustomerTimeAdjustmentItemDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetBookingTimeAdjustmentsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerTimeAdjustmentItemDto>> Handle(GetBookingTimeAdjustmentsQuery request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            if (booking.CustomerId != request.CustomerId)
                throw new InvalidOperationException("Only the booking customer can view time adjustment requests.");

            var adjustments = await _context.BookingTimeAdjustmentRequests
                .AsNoTracking()
                .Where(x => x.BookingId == request.BookingId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CustomerTimeAdjustmentItemDto
                {
                    Id = x.Id,
                    BookingId = x.BookingId,
                    BookingNumber = booking.BookingNumber,
                    RequestedAdditionalHrs = x.RequestedAdditionalHrs,
                    AdditionalCostAmount = x.AdditionalCostAmount,
                    Status = x.Status.ToString(),
                    StatusAr = TimeAdjustmentDisplayHelper.GetStatusAr(x.Status),
                    ProviderNote = x.ProviderNote,
                    CustomerAcknowledgedAt = x.CustomerAcknowledgedAt,
                    CreatedAt = x.CreatedAt,
                    CanApprove = x.Status == BookingTimeAdjustmentStatus.Pending,
                    CanReject = x.Status == BookingTimeAdjustmentStatus.Pending,
                    CanPay = x.Status == BookingTimeAdjustmentStatus.PendingPayment
                })
                .ToListAsync(cancellationToken);

            return adjustments;
        }
    }
}