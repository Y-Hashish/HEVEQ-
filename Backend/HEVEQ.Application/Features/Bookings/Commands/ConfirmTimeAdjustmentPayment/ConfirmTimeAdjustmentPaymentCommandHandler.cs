using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Payments;
using HEVEQ.Application.Common.Services;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HEVEQ.Application.Features.Bookings.Commands.ConfirmTimeAdjustmentPayment
{
    public sealed class ConfirmTimeAdjustmentPaymentCommandHandler : IRequestHandler<ConfirmTimeAdjustmentPaymentCommand, TimeAdjustmentPaymentConfirmResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly PaymentPlatformOptions _paymentOptions;
        private readonly NotificationHelper _notificationHelper;

        public ConfirmTimeAdjustmentPaymentCommandHandler( IApplicationDbContext context, IOptions<PaymentPlatformOptions> paymentOptions, NotificationHelper notificationHelper)
        {
            _context = context;
            _paymentOptions = paymentOptions.Value;
            _notificationHelper = notificationHelper;
        }

        public async Task<TimeAdjustmentPaymentConfirmResponseDto> Handle( ConfirmTimeAdjustmentPaymentCommand request, CancellationToken cancellationToken)
        {
            var adjustment = await _context.BookingTimeAdjustmentRequests
                .Include(x => x.EscrowRecords)
                .Include(x => x.Booking)
                    .ThenInclude(x => x.ServiceListing)
                        .ThenInclude(x => x.ProviderProfile)
                .FirstOrDefaultAsync(x => x.Id == request.TimeAdjustmentRequestId, cancellationToken);

            if (adjustment is null)
                throw new InvalidOperationException("Time adjustment request was not found.");

            var booking = adjustment.Booking;

            if (booking.CustomerId != request.CustomerId)
                throw new UnauthorizedAccessException("Only the booking customer can confirm time adjustment payment.");

            if (adjustment.Status != BookingTimeAdjustmentStatus.PendingPayment)
                throw new InvalidOperationException("Time adjustment is not waiting for payment confirmation.");

            if (adjustment.AdditionalCostAmount <= 0)
                throw new InvalidOperationException("Time adjustment amount is invalid.");

            var existingEscrow = adjustment.EscrowRecords
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (existingEscrow is not null)
                throw new InvalidOperationException("Escrow record already exists for this time adjustment.");

            var now = DateTime.UtcNow;

            var commissionRate = _paymentOptions.PlatformCommissionRate;
            var platformCommission = Math.Round(adjustment.AdditionalCostAmount * commissionRate, 2);
            var providerPayout = adjustment.AdditionalCostAmount - platformCommission;

            var escrow = new EscrowRecord
            {
                Id = Guid.NewGuid(),
                BookingId = null,
                MarketplaceOrderId = null,
                AdjustmentRequestId = adjustment.Id,
                GrossAmount = adjustment.AdditionalCostAmount,
                CommissionRateSnapshot = commissionRate,
                PlatformCommission = platformCommission,
                ProviderPayout = providerPayout,
                VatAmount = 0,
                PartialSettleCustomerAmt = null,
                PartialSettleProviderAmt = null,
                Status = EscrowStatus.Held,
                PaymentGatewayReference = string.IsNullOrWhiteSpace(request.PaymentGatewayReference) ? $"sim_TimeAdjustment_{adjustment.Id}" : request.PaymentGatewayReference,
                AdditionalHoldDays = _paymentOptions.DefaultEscrowHoldDays,
                EarliestReleaseAt = now.AddDays(_paymentOptions.DefaultEscrowHoldDays),
                CapturedAt = now,
                HeldAt = now,
                ReleasedAt = null,
                FrozenAt = null,
                FreezeReason = null,
                CreatedAt = now
            };

            _context.EscrowRecords.Add(escrow);

            adjustment.Status = BookingTimeAdjustmentStatus.PaymentCaptured;

            booking.EstimatedDurationHours += adjustment.RequestedAdditionalHrs;
            booking.EstimatedTotal += adjustment.AdditionalCostAmount;

            _notificationHelper.TimeAdjustmentApproved( booking.ServiceListing.ProviderProfile.UserId, adjustment.Id, booking.BookingNumber);

            await _context.SaveChangesAsync(cancellationToken);

            return new TimeAdjustmentPaymentConfirmResponseDto
            {
                TimeAdjustmentRequestId = adjustment.Id,
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                TimeAdjustmentStatus = adjustment.Status.ToString(),
                TimeAdjustmentStatusAr = TimeAdjustmentDisplayHelper.GetStatusAr(adjustment.Status),
                EscrowStatus = escrow.Status.ToString(),
                EscrowStatusAr = EscrowDisplayHelper.GetStatusAr(escrow.Status),
                BookingEstimatedDurationHours = booking.EstimatedDurationHours,
                BookingEstimatedTotal = booking.EstimatedTotal,
                Message = "Additional time adjustment payment captured successfully"
            };
        }
    }
}