using System.Threading;
using HEVEQ.Application.Common.AI.Models;

namespace HEVEQ.Application.Common.AI.Interfaces.PreBooking;
public interface IPreBookingFeasibilityEvaluator
{
    /// <param name="context">Flattened booking/provider snapshot.</param>
    /// <param name="thread">The run's shared conversation thread. The Concierge's turn is the
    /// first message written to it.</param>
    Task<AgentTurnResult> EvaluateAsync(
        PreBookingContext context,
        IAgentConversationThread thread,
        CancellationToken ct = default);
}