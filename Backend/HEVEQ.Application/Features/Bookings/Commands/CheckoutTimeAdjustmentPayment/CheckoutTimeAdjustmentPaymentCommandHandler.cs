using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Payments;
using HEVEQ.Application.Features.Bookings.DTOs;
using HEVEQ.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HEVEQ.Application.Features.Bookings.Commands.CheckoutTimeAdjustmentPayment
{
    public sealed class CheckoutTimeAdjustmentPaymentCommandHandler : IRequestHandler<CheckoutTimeAdjustmentPaymentCommand, TimeAdjustmentPaymentCheckoutResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPaymentCheckoutService _paymentCheckoutService;

        public CheckoutTimeAdjustmentPaymentCommandHandler( IApplicationDbContext context, IPaymentCheckoutService paymentCheckoutService)
        {
            _context = context;
            _paymentCheckoutService = paymentCheckoutService;
        }

        public async Task<TimeAdjustmentPaymentCheckoutResponseDto> Handle( CheckoutTimeAdjustmentPaymentCommand request, CancellationToken cancellationToken)
        {
            var adjustment = await _context.BookingTimeAdjustmentRequests
                .Include(x => x.Booking)
                .FirstOrDefaultAsync(x => x.Id == request.TimeAdjustmentRequestId, cancellationToken);

            if (adjustment is null)
                throw new InvalidOperationException("Time adjustment request was not found.");

            var booking = adjustment.Booking;

            if (booking.CustomerId != request.CustomerId)
                throw new UnauthorizedAccessException("Only the booking customer can pay for this time adjustment.");

            if (adjustment.Status != BookingTimeAdjustmentStatus.PendingPayment)
                throw new InvalidOperationException("Time adjustment is not waiting for payment.");

            if (adjustment.AdditionalCostAmount <= 0)
                throw new InvalidOperationException("Time adjustment amount is invalid.");

            var checkout = await _paymentCheckoutService.CreateCheckoutAsync(
                new PaymentCheckoutCreateRequest
                {
                    ReferenceType = PaymentReferenceType.TimeAdjustment,
                    ReferenceId = adjustment.Id,
                    ReferenceNumber = $"{booking.BookingNumber}-ADJ",
                    PayingUserId = request.CustomerId,
                    Amount = adjustment.AdditionalCostAmount,
                    Currency = "EGP",
                    PaymentMethod = request.PaymentMethod,
                    SuccessUrl = request.SuccessUrl,
                    CancelUrl = request.CancelUrl
                },
                cancellationToken);

            return new TimeAdjustmentPaymentCheckoutResponseDto
            {
                TimeAdjustmentRequestId = adjustment.Id,
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber,
                Amount = adjustment.AdditionalCostAmount,
                Currency = "EGP",
                PaymentProvider = checkout.PaymentProvider,
                CheckoutUrl = checkout.CheckoutUrl,
                Status = checkout.Status,
                PaymentGatewayReference = checkout.PaymentGatewayReference
            };
        }
    }
}