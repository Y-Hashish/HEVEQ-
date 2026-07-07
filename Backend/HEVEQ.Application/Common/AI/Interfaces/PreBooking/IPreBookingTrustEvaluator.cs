using HEVEQ.Application.Common.AI.Models;

namespace HEVEQ.Application.Common.AI.Interfaces.PreBooking;

public interface IPreBookingTrustEvaluator
{
    /// <param name="context">Flattened booking/provider snapshot (same instance Concierge saw).</param>
    /// <param name="feasibilityVerdict">The Concierge's Turn 1 result.</param>
    /// <param name="thread">The same shared conversation thread the Concierge just wrote to.</param>
    Task<AgentTurnResult> EvaluateAsync(
        PreBookingContext context,
        AgentTurnResult feasibilityVerdict,
        IAgentConversationThread thread,
        CancellationToken ct = default);
}