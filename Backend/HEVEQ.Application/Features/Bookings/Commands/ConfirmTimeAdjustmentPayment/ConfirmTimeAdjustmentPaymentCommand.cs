using HEVEQ.Application.Features.Bookings.DTOs;
using MediatR;

namespace HEVEQ.Application.Features.Bookings.Commands.ConfirmTimeAdjustmentPayment
{
    public sealed record ConfirmTimeAdjustmentPaymentCommand(Guid CustomerId, Guid TimeAdjustmentRequestId, string? PaymentGatewayReference) : IRequest<TimeAdjustmentPaymentConfirmResponseDto>;
}