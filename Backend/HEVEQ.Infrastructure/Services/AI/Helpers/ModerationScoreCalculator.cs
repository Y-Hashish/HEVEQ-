using HEVEQ.Application.Common.AI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.Services.AI.Helpers
{
    public static class ModerationScoreCalculator
    {
        public static int Compute(int aiScore, PriceAnomalyDetail price, ContactLeakDetail contact, bool spam)
        {
            var score = aiScore;

            if (price.IsAnomalous) score += 20;
            if (contact.ContainsPhoneNumber || contact.ContainsEmail) score += 30;
            if (contact.ContainsExternalPlatformReference) score += 25;
            if (spam) score += 15;
            return Math.Min(score, 100);
        }
        public static int Compute(int aiScore, ServicePriceAnalysisResult? price, ContactLeakDetail contact, bool spam)
        {
            var score = aiScore;
            if (price?.Hourly?.IsAnomalous == true ||price?.Daily?.IsAnomalous == true){score += 20;}
            if (contact.ContainsPhoneNumber || contact.ContainsEmail) score += 30;
            if (contact.ContainsExternalPlatformReference) score += 25;
            if (spam) score += 15;
            return Math.Min(score, 100);
        }
        public static RiskLevel ToRiskLevel(int score) => score switch
        {
            >= 70 => RiskLevel.High,
            >= 40 => RiskLevel.Medium,
            _ => RiskLevel.Low
        };

        public static ModerationRecommendation ToRecommendation(int score) => score switch
        {
            >= 70 => ModerationRecommendation.Reject,
            >= 40 => ModerationRecommendation.ApproveWithConditions,
            _ => ModerationRecommendation.Approve
        };

    }
}
