using System.Threading;
using System.Threading.Tasks;
using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
using HEVEQ.Application.Common.AI.Models;
using MediatR;

namespace HEVEQ.Application.Features.Bookings.PreBookingEvaluation;

public sealed record EvaluatePreBookingCommand(Guid BookingId) : IRequest<PreBookingDecisionResult>;

public sealed class EvaluatePreBookingCommandHandler : IRequestHandler<EvaluatePreBookingCommand, PreBookingDecisionResult>
{
    private readonly IPreBookingOrchestrator _orchestrator;

    public EvaluatePreBookingCommandHandler(IPreBookingOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public Task<PreBookingDecisionResult> Handle(EvaluatePreBookingCommand request, CancellationToken cancellationToken)
        => _orchestrator.EvaluateAsync(request.BookingId, cancellationToken);
}