using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
using Microsoft.SemanticKernel.Agents;

namespace HEVEQ.Infrastructure.AI.Agents;

/// <summary>
/// One instance = one pre-booking evaluation run. Wraps a single ChatHistoryAgentThread so
/// PreBookingOrchestrationService can create it once and hand the SAME instance to both
/// ConciergeFeasibilityEvaluator (turn 1) and ModeratorTrustEvaluator (turn 2) — the Moderator's
/// agent literally sees the Concierge's turn in the underlying SK chat history.
/// </summary>
public sealed class PreBookingAgentThreadAdapter : IAgentConversationThread
{
    public ChatHistoryAgentThread SemanticKernelThread { get; } = new();
}