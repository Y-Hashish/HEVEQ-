using HEVEQ.Application.Common.AI.Interfaces;
using HEVEQ.Application.Common.AI.Models;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Domain.Entities;
using HEVEQ.Domain.Enums;
using HEVEQ.Infrastructure.Services.AI.ModerationAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace HEVEQ.Infrastructure.Services.AI.BackgroundJobs
{
    public class MarketplaceModerationJob(
    IApplicationDbContext context,
    MarketplaceListingRiskAnalyzer analyzer,
    ILogger<MarketplaceModerationJob> logger)
    {
        // Hangfire calls this method directly — no polling, no loop.
        // Each listing gets its own fire-and-forget job, enqueued by the command handler.
        public async Task RunAsync(Guid listingId, CancellationToken ct = default)
        {
            var listing = await context.MarketplaceListings
                 .FirstOrDefaultAsync(l => l.Id == listingId, ct);

            if (listing is null)
            {
                logger.LogWarning("MarketplaceModerationJob: listing {ListingId} not found — skipping.", listingId);
                return;
            }
            var sw = Stopwatch.StartNew();

            ListingRiskReport report;
            try
            {
                report = await analyzer.AnalyzeMarketplaceListingAsync(listingId, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MarketplaceModerationJob: AI analysis failed for listing {ListingId}.", listingId);
                // Hangfire will automatically retry based on the [AutomaticRetry] attribute below.
                throw;
            }

            sw.Stop();
            listing.AiRiskScore = report.RiskScore;
            listing.AiRiskLevel = report.RiskLevel.ToString();

            var flagParts = new List<string>();
            flagParts.AddRange(report.Flags);
            flagParts.AddRange(report.PhotoFlags);

            if (!string.IsNullOrWhiteSpace(report.PriceAnomalyReason))
                flagParts.Add($"Price: {report.PriceAnomalyReason}");

            if (!string.IsNullOrWhiteSpace(report.AdminNote))
                flagParts.Add($"Note: {report.AdminNote}");

            listing.AiRiskFlags = flagParts.Count > 0 ? string.Join(" | ", flagParts) : null;

            context.AiInteractionLogs.Add(new AiInteractionLog
            {
                AgentType = AiAgentType.Moderator,
                InvocationContext = "MarketplaceListing.Moderation",
                EntityType = nameof(MarketplaceListing),
                EntityId = listingId,
                AiRecommendation = report.Recommendation.ToString(),
                AiRiskScore = report.RiskScore,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow,
                //ExpiresAt = DateTime.UtcNow.AddDays(90)
            });

            await context.SaveChangesAsync(ct);

            logger.LogInformation(
             "MarketplaceModerationJob: Listing {ListingId} scored {Score} ({Level}). Recommendation={Recommendation}",
             listingId,
             report.RiskScore,
             report.RiskLevel,
             report.Recommendation);

        }

    }
}
