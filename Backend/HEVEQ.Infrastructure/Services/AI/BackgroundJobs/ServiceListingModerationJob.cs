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
    public class ServiceListingModerationJob(
    IApplicationDbContext context,
    ServiceListingRiskAnalyzer analyzer,
    ILogger<ServiceListingModerationJob> logger)
    {
        public async Task RunAsync(Guid listingId, CancellationToken ct = default)
        {
            var listing = await context.ServiceListings
           .FirstOrDefaultAsync(x => x.Id == listingId, ct);

            if (listing is null)
            {
                logger.LogWarning(
                    "ServiceListingModerationJob: Listing {ListingId} not found.",
                    listingId);

                return;
            }

            var sw = Stopwatch.StartNew();

            ListingRiskReport report;

            try
            {
                report = await analyzer.AnalyzeServiceListingAsync(listingId, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Service moderation failed for {ListingId}",
                    listingId);

                throw;
            }

            sw.Stop();

            listing.AiRiskScore = report.RiskScore;
            listing.AiRiskLevel = report.RiskLevel.ToString();
            listing.AiRecommendation = report.Recommendation.ToString();

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
                InvocationContext = "ServiceListing.Moderation",
                EntityType = nameof(ServiceListing),
                EntityId = listingId,
                AiRecommendation = report.Recommendation.ToString(),
                AiRiskScore = report.RiskScore,
                LatencyMs = (int)sw.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync(ct);

            logger.LogInformation(
                "ServiceListingModerationJob: Listing {ListingId} scored {Score} ({Level}). Recommendation={Recommendation}",
                listingId,
                report.RiskScore,
                report.RiskLevel,
                report.Recommendation);


        }
    }
}
