using System.Text;
using System.Text.RegularExpressions;
using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Infrastructure.AI.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace HEVEQ.Infrastructure.AI.Agents;

public sealed class ConciergeFeasibilityEvaluator : IPreBookingFeasibilityEvaluator
{
    private const string SystemPrompt = """
        You are the Concierge Agent for HEVEQ, a heavy-equipment rental marketplace in Egypt.
        You evaluate ONLY logistics feasibility for one booking: geo-fencing, scheduling,
        availability, and operator conflicts. You NEVER evaluate provider trust, fraud, or
        safety history - that is the Moderator Agent's job, not yours.

        Call check_geo_fence and check_schedule_availability for the given bookingId before
        answering. Base your verdict only on what those functions return - never guess.

        Respond in exactly this format:
        VERDICT: PROCEED | PROCEED_WITH_WARNING | BLOCK
        <one short paragraph in Arabic explaining why, referencing the concrete facts you
        retrieved: distance vs radius, availability window, blackout date, operator conflict>

        Rules:
        - BLOCK if outside the provider's service radius AND no out-of-zone surcharge was
          accepted for this booking.
        - BLOCK if the requested slot falls outside listed availability hours, lands on a
          blackout date, or the assigned operator has a scheduling conflict.
        - PROCEED_WITH_WARNING if outside the service radius but the out-of-zone surcharge was
          already accepted - not a hard blocker, just a heads-up for the Moderator/Admin.
        - PROCEED only if every check above comes back clean.
        """;

    private readonly Kernel _kernel;

    public ConciergeFeasibilityEvaluator(Kernel kernel, ConciergeGeoAvailabilityPlugin plugin)
    {
        _kernel = kernel.Clone();
        _kernel.Plugins.AddFromObject(plugin, "ConciergeTools");
    }

    public async Task<AgentTurnResult> EvaluateAsync(
        PreBookingContext context,
        IAgentConversationThread thread,
        CancellationToken ct = default)
    {
        var skThread = ((PreBookingAgentThreadAdapter)thread).SemanticKernelThread;

        var agent = new ChatCompletionAgent
        {
            Name = "ConciergeAgent",
            Instructions = SystemPrompt,
            Kernel = _kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }),
        };

        var briefing =
            $"Evaluate booking {context.BookingId} (\"{context.JobTitle}\"). " +
            $"Requested start: {context.RequestedStartDate:yyyy-MM-dd} {context.RequestedStartTime:HH:mm}, " +
            $"duration {context.EstimatedDurationHours}h. " +
            $"Provider service radius: {context.ProviderServiceRadiusKm} km.";

        var responseText = new StringBuilder();
        await foreach (var response in agent.InvokeAsync(
            new ChatMessageContent(AuthorRole.User, briefing), skThread, cancellationToken: ct))
        {
            responseText.Append(response.Message.Content);
        }

        return ParseVerdict("ConciergeAgent", responseText.ToString());
    }

    private static AgentTurnResult ParseVerdict(string agentName, string rawText)
    {
        var match = Regex.Match(rawText, @"VERDICT:\s*(PROCEED_WITH_WARNING|PROCEED|BLOCK)",
            RegexOptions.IgnoreCase);

        // Fail-closed: if the model didn't answer in the required format, treat it as a BLOCK
        // rather than silently letting a malformed response through as a PROCEED.
        var verdict = match.Success ? match.Groups[1].Value.ToUpperInvariant() : "BLOCK";
        return new AgentTurnResult(agentName, verdict, rawText);
    }
}