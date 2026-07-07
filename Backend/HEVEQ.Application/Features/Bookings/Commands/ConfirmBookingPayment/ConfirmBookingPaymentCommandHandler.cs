using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Payments;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Application.Features.Bookings.Helpers;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HEVEQ.Application.Common.Services;

namespace HEVEQ.Application.Features.Bookings.Commands.ConfirmBookingPayment
{
    public sealed class ConfirmBookingPaymentCommandHandler : IRequestHandler<ConfirmBookingPaymentCommand, BookingPaymentConfirmResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly PaymentPlatformOptions _paymentOptions;
        private readonly NotificationHelper _notificationHelper;

        public ConfirmBookingPaymentCommandHandler(IApplicationDbContext context, IOptions<PaymentPlatformOptions> paymentOptions, NotificationHelper notificationHelper)
        {
            _context = context;
            _paymentOptions = paymentOptions.Value;
            _notificationHelper = notificationHelper;
        }

        public async Task<BookingPaymentConfirmResponseDto> Handle(ConfirmBookingPaymentCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .Include(x => x.EscrowRecords)
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .FirstOrDefaultAsync(x => x.Id == request.BookingId, cancellationToken);

            if (booking is null)
                throw new InvalidOperationException("Booking was not found.");

            if (booking.CustomerId != request.CustomerId)
                throw new UnauthorizedAccessException("Only the booking customer can confirm payment for this booking.");

            if (booking.Status != BookingStatus.ConfirmedPendingPayment)
                throw new InvalidOperationException("Booking is not ready for payment confirmation.");

            if (booking.EstimatedTotal <= 0)
                throw new InvalidOperationException("Booking amount is invalid.");

            var existingEscrow = booking.EscrowRecords
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (existingEscrow is not null)
                throw new InvalidOperationException("Escrow record already exists for this booking.");

            var now = DateTime.UtcNow;

            var commissionRate = _paymentOptions.PlatformCommissionRate;
            var platformCommission = Math.Round(booking.EstimatedTotal * commissionRate, 2);
            var providerPayout = booking.EstimatedTotal - platformCommission;

            var escrow = new EscrowRecord
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                MarketplaceOrderId = null,
                AdjustmentRequestId = null,

                GrossAmount = booking.EstimatedTotal,
                CommissionRateSnapshot = commissionRate,
                PlatformCommission = platformCommission,
                ProviderPayout = providerPayout,
                VatAmount = 0,

                PartialSettleCustomerAmt = null,
                PartialSettleProviderAmt = null,

                Status = EscrowStatus.Held,
                PaymentGatewayReference = string.IsNullOrWhiteSpace(request.PaymentGatewayReference) ? $"sim_Booking_{booking.Id}" : request.PaymentGatewayReference,
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

            booking.PaymentCapturedAt = now;
            booking.Status = BookingStatus.Active;

            _notificationHelper.BookingPaymentCaptured(booking.ServiceListing.ProviderProfile.UserId, booking.Id, booking.BookingNumber);
            await _context.SaveChangesAsync(cancellationToken);

            return new BookingPaymentConfirmResponseDto
            {
                BookingId = booking.Id,
                BookingStatus = booking.Status.ToString(),
                BookingStatusAr = BookingDisplayHelper.GetStatusAr(booking.Status),
                EscrowStatus = escrow.Status.ToString(),
                EscrowStatusAr = EscrowDisplayHelper.GetStatusAr(escrow.Status),
                Message = "Payment captured and booking activated"
            };
        }
    }
}