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

/// <summary>
/// Turn 2. Backs IPreBookingTrustEvaluator: provider TrustScore/TrustLevel, incident history, and
/// open fraud/safety tickets. Never reasons about geo-fencing/scheduling - that already happened
/// in the Concierge's turn, which this class receives on the same shared thread and cannot
/// override (a Concierge BLOCK is enforced in code below, not just by prompt).
/// </summary>
public sealed class ModeratorTrustEvaluator : IPreBookingTrustEvaluator
{
    private const string SystemPrompt = """
        You are the Moderator Agent for HEVEQ. You evaluate ONLY provider trust standing,
        incident history, and open fraud/safety tickets for one booking. You NEVER evaluate
        geo-fencing, scheduling, or availability - the Concierge Agent already handled that in
        the turn right before yours.

        Call get_provider_trust_profile, get_provider_incidents, and get_open_provider_tickets
        for the given bookingId before answering. Base your verdict only on what those
        functions return - never guess.

        You will be told the Concierge Agent's verdict. If it was BLOCK, your verdict must also
        be BLOCK - a logistics block is never overridden by good trust standing. Otherwise,
        decide independently from the trust signals.

        Respond in exactly this format:
        VERDICT: PROCEED | PROCEED_WITH_WARNING | BLOCK
        <one short paragraph in Arabic explaining why, referencing the concrete trust score,
        trust level, incident count, and open ticket count you retrieved>

        Rules:
        - BLOCK if TrustLevel is AtRisk, or there are 2+ safety-issue incidents in the last 90
          days, or any open Fraud ticket.
        - PROCEED_WITH_WARNING if TrustLevel is Standard with an open SafetyConcern ticket, or
          exactly 1 safety-issue incident in the last 90 days.
        - PROCEED if TrustLevel is Verified/TopProvider with no open tickets and no safety
          incidents in the last 90 days.
        """;

    private readonly Kernel _kernel;

    public ModeratorTrustEvaluator(Kernel kernel, ModeratorTrustIncidentPlugin plugin)
    {
        _kernel = kernel.Clone();
        _kernel.Plugins.AddFromObject(plugin, "ModeratorTools");
    }

    public async Task<AgentTurnResult> EvaluateAsync(
        PreBookingContext context,
        AgentTurnResult feasibilityVerdict,
        IAgentConversationThread thread,
        CancellationToken ct = default)
    {
        var skThread = ((PreBookingAgentThreadAdapter)thread).SemanticKernelThread;

        var agent = new ChatCompletionAgent
        {
            Name = "ModeratorAgent",
            Instructions = SystemPrompt,
            Kernel = _kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }),
        };

        var briefing =
            $"Evaluate booking {context.BookingId} for provider {context.ProviderProfileId}. " +
            $"Concierge Agent's verdict was: {feasibilityVerdict.Verdict}.";

        var responseText = new StringBuilder();
        await foreach (var response in agent.InvokeAsync(
            new ChatMessageContent(AuthorRole.User, briefing), skThread, cancellationToken: ct))
        {
            responseText.Append(response.Message.Content);
        }

        var parsed = ParseVerdict("ModeratorAgent", responseText.ToString());

        // Defense in depth: enforce the non-override rule in code, not just via prompt wording.
        if (feasibilityVerdict.Verdict == "BLOCK" && parsed.Verdict != "BLOCK")
        {
            return parsed with { Verdict = "BLOCK" };
        }

        return parsed;
    }

    private static AgentTurnResult ParseVerdict(string agentName, string rawText)
    {
        var match = Regex.Match(rawText, @"VERDICT:\s*(PROCEED_WITH_WARNING|PROCEED|BLOCK)",
            RegexOptions.IgnoreCase);

        var verdict = match.Success ? match.Groups[1].Value.ToUpperInvariant() : "BLOCK";
        return new AgentTurnResult(agentName, verdict, rawText);
    }
}