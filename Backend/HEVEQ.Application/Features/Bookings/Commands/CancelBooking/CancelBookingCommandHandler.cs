using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Application.Features.Bookings.Services;
using HEVEQ.Application.Features.Bookings.Services.Interfaces;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Bookings.Commands.CancelBooking
{
    public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICancellationPolicyService _policyService;
        private readonly NotificationHelper _notificationHelper;
        public CancelBookingCommandHandler(IApplicationDbContext context, ICancellationPolicyService policyService, NotificationHelper notificationHelper)
        {
            _context = context;
            _policyService = policyService;
            _notificationHelper = notificationHelper;
        }

        public async Task<CancelBookingResponseDto> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            var initiator = await _policyService.ResolveActorAsync(booking, request.UserId, cancellationToken
            );

            if (booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Completed booking cannot be cancelled.");

            if (booking.Status == BookingStatus.InProgress && initiator == BookingCancellationInitiator.Customer)
                throw new InvalidOperationException("Customer cannot cancel an in-progress booking directly.");

            _policyService.EnsureCanBeCancelled(booking.Status);

            var refundPercentage = _policyService.CalculateRefundPercentage(booking.Status);
            var hasCapturedPayment = booking.PaymentCapturedAt is not null;

            booking.Status = hasCapturedPayment ? BookingStatus.CancelledRefunded : BookingStatus.Cancelled;
            booking.CancellationReason = request.Reason;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationInitiatedByRole = initiator;
            booking.CancellationRefundPct = refundPercentage;

            if (initiator == BookingCancellationInitiator.Provider && hasCapturedPayment)
                booking.ProviderCancellationPenaltyApplied = true;

            var assignment = await _context.OperatorAssignments
                .FirstOrDefaultAsync(x => x.BookingId == booking.Id, cancellationToken);

            if (assignment is not null &&assignment.Status != OperatorAssignmentStatus.Completed && assignment.Status != OperatorAssignmentStatus.Cancelled)
                assignment.Status = OperatorAssignmentStatus.Cancelled;

            var escrow = await _context.EscrowRecords
                .FirstOrDefaultAsync(x => x.BookingId == booking.Id, cancellationToken);

            if (escrow is not null && hasCapturedPayment)
            {
                if (refundPercentage > 0)
                {
                    escrow.Status = EscrowStatus.Refunded;
                }
                else
                {
                    escrow.Status = EscrowStatus.Frozen;
                    escrow.FrozenAt = DateTime.UtcNow;
                    escrow.FreezeReason = "Booking cancelled with zero refund percentage.";
                    //TODO : Implement logic to handle the case where the refund percentage is zero, such as notifying the provider or taking further action.
                }
            }

            var pendingAdjustments = await _context.BookingTimeAdjustmentRequests
                .Where(x =>
                    x.BookingId == booking.Id &&
                    x.Status == BookingTimeAdjustmentStatus.Pending)
                .ToListAsync(cancellationToken);

            foreach (var adjustment in pendingAdjustments)
            {
                adjustment.Status = BookingTimeAdjustmentStatus.Expired;
            }

            var notificationUserId = initiator == BookingCancellationInitiator.Customer ? booking.ServiceListing.ProviderProfile.UserId : booking.CustomerId;

            var cancelledBy = initiator == BookingCancellationInitiator.Customer ? "العميل" : "مزود الخدمة";

            _notificationHelper.BookingCancelled(notificationUserId, booking.Id, booking.BookingNumber, cancelledBy);
            await _context.SaveChangesAsync(cancellationToken);

            return new CancelBookingResponseDto
            {
                BookingId = booking.Id,
                Status = booking.Status.ToString(),
                StatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                RefundPercentage = refundPercentage,
                Message = "Booking cancelled successfully"
            };
        }
    }
}