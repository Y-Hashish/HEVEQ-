using HEVEQ.Application.Features.Bookings.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Bookings.Commands.CheckoutTimeAdjustmentPayment
{
    public sealed record CheckoutTimeAdjustmentPaymentCommand(Guid CustomerId, Guid TimeAdjustmentRequestId, string PaymentMethod, string? SuccessUrl, string? CancelUrl) : IRequest<TimeAdjustmentPaymentCheckoutResponseDto>;
}