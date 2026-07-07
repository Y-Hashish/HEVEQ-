using System.ComponentModel;
using System.Text.Json;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

namespace HEVEQ.Infrastructure.AI.Plugins;

/// <summary>
/// Moderator's data-access surface. Every function queries EF Core directly and returns plain
/// JSON facts — no reasoning happens here. The ModeratorAgent (ChatCompletionAgent) decides what
/// the facts mean; this plugin never computes a verdict.
/// Owned exclusively by Moderator — Concierge never references this plugin.
/// </summary>
public sealed class ModeratorTrustIncidentPlugin
{
    private static readonly TimeSpan IncidentLookback = TimeSpan.FromDays(90);

    private readonly IApplicationDbContext _db;

    public ModeratorTrustIncidentPlugin(IApplicationDbContext db)
    {
        _db = db;
    }

    [KernelFunction("get_provider_trust_profile")]
    [Description("Returns the provider's trust standing. JSON: { trustScore (0-100), " +
                 "trustLevel (AtRisk|Standard|Verified|TopProvider), completedBookingsCount, " +
                 "responseRate }.")]
    public async Task<string> GetProviderTrustProfileAsync(
        [Description("The booking's Id")] Guid bookingId,
        CancellationToken ct = default)
    {
        var provider = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new
            {
                b.ServiceListing.ProviderProfile.TrustScore,
                b.ServiceListing.ProviderProfile.TrustLevel,
                b.ServiceListing.ProviderProfile.CompletedBookingsCount,
                b.ServiceListing.ProviderProfile.ResponseRate
            })
            .FirstOrDefaultAsync(ct);

        if (provider is null)
        {
            return JsonSerializer.Serialize(new { error = "booking_not_found" });
        }

        return JsonSerializer.Serialize(new
        {
            trustScore = provider.TrustScore,
            trustLevel = provider.TrustLevel.ToString(),
            completedBookingsCount = provider.CompletedBookingsCount,
            responseRate = provider.ResponseRate
        });
    }

    [KernelFunction("get_provider_incidents")]
    [Description("Returns the provider's incidents from the last 90 days (unresponsiveness, late " +
                 "cancellations, penalties, safety issues). JSON: { totalCount, safetyIssueCount, " +
                 "penaltyAppliedCount, incidents: [{ incidentType, occurredAt, penaltyApplied }] }.")]
    public async Task<string> GetProviderIncidentsAsync(
        [Description("The booking's Id")] Guid bookingId,
        CancellationToken ct = default)
    {
        var providerProfileId = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => b.ServiceListing.ProviderProfileId)
            .FirstOrDefaultAsync(ct);

        if (providerProfileId == Guid.Empty)
        {
            return JsonSerializer.Serialize(new { error = "booking_not_found" });
        }

        var cutoff = DateTime.UtcNow - IncidentLookback;

        var incidents = await _db.ProviderIncidents
            .AsNoTracking()
            .Where(i => i.ProviderProfileId == providerProfileId && i.OccurredAt >= cutoff)
            .OrderByDescending(i => i.OccurredAt)
            .Select(i => new
            {
                incidentType = i.IncidentType.ToString(),
                occurredAt = i.OccurredAt,
                penaltyApplied = i.PenaltyApplied
            })
            .ToListAsync(ct);

        return JsonSerializer.Serialize(new
        {
            totalCount = incidents.Count,
            safetyIssueCount = incidents.Count(i => i.incidentType == nameof(ProviderIncidentType.SafetyIssue)),
            penaltyAppliedCount = incidents.Count(i => i.penaltyApplied),
            incidents
        });
    }

    [KernelFunction("get_open_provider_tickets")]
    [Description("Returns open Fraud or SafetyConcern tickets raised against this provider's " +
                 "bookings (Status not Resolved/Closed). JSON: { openCount, tickets: " +
                 "[{ category, status, priority, subject, createdAt }] }.")]
    public async Task<string> GetOpenProviderTicketsAsync(
        [Description("The booking's Id")] Guid bookingId,
        CancellationToken ct = default)
    {
        var providerProfileId = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => b.ServiceListing.ProviderProfileId)
            .FirstOrDefaultAsync(ct);

        if (providerProfileId == Guid.Empty)
        {
            return JsonSerializer.Serialize(new { error = "booking_not_found" });
        }

        var tickets = await _db.Tickets
            .AsNoTracking()
            .Where(t => t.Booking != null && t.Booking.ServiceListing.ProviderProfileId == providerProfileId)
            .Where(t => t.Category == TicketCategory.Fraud || t.Category == TicketCategory.SafetyConcern)
            .Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                category = t.Category.ToString(),
                status = t.Status.ToString(),
                priority = t.Priority,
                subject = t.Subject,
                createdAt = t.CreatedAt
            })
            .ToListAsync(ct);

        return JsonSerializer.Serialize(new
        {
            openCount = tickets.Count,
            tickets
        });
    }
}