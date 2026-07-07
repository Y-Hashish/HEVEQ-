using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Bookings.Commands.ApproveTimeAdjustment
{
    public sealed class ApproveTimeAdjustmentCommandHandler : IRequestHandler<ApproveTimeAdjustmentCommand, TimeAdjustmentDecisionResponseDto>
    {
        private readonly IApplicationDbContext _context;

        public ApproveTimeAdjustmentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TimeAdjustmentDecisionResponseDto> Handle(ApproveTimeAdjustmentCommand request, CancellationToken cancellationToken)
        {
            var adjustmentRequest = await _context.BookingTimeAdjustmentRequests
                .Include(x => x.Booking)
                    .ThenInclude(x => x.ServiceListing)
                        .ThenInclude(x => x.ProviderProfile)
                .FirstOrDefaultAsync(x => x.Id == request.TimeAdjustmentRequestId, cancellationToken);

            if (adjustmentRequest is null)
                throw new InvalidOperationException("Time adjustment request was not found.");

            var booking = adjustmentRequest.Booking;

            if (booking is null)
                throw new InvalidOperationException("Related booking was not found.");

            if (booking.CustomerId != request.CustomerId)
                throw new InvalidOperationException("Only the booking customer can approve this time adjustment request.");

            if (adjustmentRequest.Status != BookingTimeAdjustmentStatus.Pending)
                throw new InvalidOperationException("Only pending time adjustment requests can be approved.");

            if (booking.Status != BookingStatus.Active && booking.Status != BookingStatus.InProgress && booking.Status != BookingStatus.PendingCustomerConfirmation)
                throw new InvalidOperationException("Time adjustment can only be approved before booking completion.");

            adjustmentRequest.Status = BookingTimeAdjustmentStatus.PendingPayment;
            adjustmentRequest.CustomerAcknowledgedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new TimeAdjustmentDecisionResponseDto
            {
                Id = adjustmentRequest.Id,
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                RequestedAdditionalHrs = adjustmentRequest.RequestedAdditionalHrs,
                AdditionalCostAmount = adjustmentRequest.AdditionalCostAmount,
                BookingEstimatedDurationHours = booking.EstimatedDurationHours,
                BookingEstimatedTotal = booking.EstimatedTotal,
                Status = adjustmentRequest.Status.ToString(),
                StatusAr = TimeAdjustmentDisplayHelper.GetStatusAr(adjustmentRequest.Status),
                CustomerAcknowledgedAt = adjustmentRequest.CustomerAcknowledgedAt,
                Message = "Time adjustment approved and waiting for additional payment"
            };
        }
    }
}