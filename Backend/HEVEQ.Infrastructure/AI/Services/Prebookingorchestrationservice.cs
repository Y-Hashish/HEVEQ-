using System.Diagnostics;
using System.Threading;
using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Interfaces.PreBooking;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Infrastructure.AI.Agents;

namespace HEVEQ.Infrastructure.AI.Services;
public sealed class PreBookingOrchestrationService : IPreBookingOrchestrator
{
    private readonly IBookingReadRepository _bookingReadRepository;
    private readonly IPreBookingFeasibilityEvaluator _feasibilityEvaluator;
    private readonly IPreBookingTrustEvaluator _trustEvaluator;
    private readonly IApplicationDbContext _db;

    public PreBookingOrchestrationService(
        IBookingReadRepository bookingReadRepository,
        IPreBookingFeasibilityEvaluator feasibilityEvaluator,
        IPreBookingTrustEvaluator trustEvaluator,
        IApplicationDbContext db)
    {
        _bookingReadRepository = bookingReadRepository;
        _feasibilityEvaluator = feasibilityEvaluator;
        _trustEvaluator = trustEvaluator;
        _db = db;
    }

    public async Task<PreBookingDecisionResult> EvaluateAsync(Guid bookingId, CancellationToken ct = default)
    {
        var context = await _bookingReadRepository.GetPreBookingContextAsync(bookingId, ct)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found for pre-booking evaluation.");

        var thread = new PreBookingAgentThreadAdapter();

        // Turn 1 - Concierge
        var conciergeClock = Stopwatch.StartNew();
        var feasibilityVerdict = await _feasibilityEvaluator.EvaluateAsync(context, thread, ct);
        conciergeClock.Stop();
        await LogInteractionAsync(AiAgentType.Concierge, bookingId, feasibilityVerdict, conciergeClock.ElapsedMilliseconds, ct);

        // Turn 2 - Moderator (reads Concierge's turn from the same shared thread)
        var moderatorClock = Stopwatch.StartNew();
        var trustVerdict = await _trustEvaluator.EvaluateAsync(context, feasibilityVerdict, thread, ct);
        moderatorClock.Stop();
        await LogInteractionAsync(AiAgentType.Moderator, bookingId, trustVerdict, moderatorClock.ElapsedMilliseconds, ct);

        var decision = CombineVerdicts(feasibilityVerdict, trustVerdict);
        var turns = new List<AgentTurnResult> { feasibilityVerdict, trustVerdict };

        return new PreBookingDecisionResult(
            BookingId: bookingId,
            Decision: decision,
            Turns: turns,
            FinalRationale: $"Concierge: {feasibilityVerdict.Verdict}. Moderator: {trustVerdict.Verdict}.");
    }

    private static PreBookingDecision CombineVerdicts(AgentTurnResult feasibility, AgentTurnResult trust)
    {
        if (feasibility.Verdict == "BLOCK" || trust.Verdict == "BLOCK")
        {
            return PreBookingDecision.Block;
        }

        if (feasibility.Verdict == "PROCEED_WITH_WARNING" || trust.Verdict == "PROCEED_WITH_WARNING")
        {
            return PreBookingDecision.ProceedWithWarning;
        }

        return PreBookingDecision.Proceed;
    }

    private async Task LogInteractionAsync(
        AiAgentType agentType, Guid bookingId, AgentTurnResult turn, long latencyMs, CancellationToken ct)
    {
     
        _db.AiInteractionLogs.Add(new AiInteractionLog
        {
            AgentType = agentType,
            InvocationContext = "PreBookingEvaluation",
            EntityType = nameof(Booking),
            EntityId = bookingId,
            AiRecommendation = turn.Verdict,
            LatencyMs = (int)latencyMs,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
    }
}