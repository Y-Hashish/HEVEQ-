//using HEVEQ.Application.Common.AI.Models;
//using HEVEQ.Application.Common.Interfaces;
//using HEVEQ.Domain.Enums;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace HEVEQ.Infrastructure.Services.AI.Helpers
//{
//    public class ServiceListingPriceAnomalyDetector(IApplicationDbContext context)
//    {
//        private const decimal LowPriceThresholdPct = 0.40m;
//        private const decimal HighPriceThresholdPct = 2.00m;

//        public async Task<ServicePriceAnalysisResult> AnalyzeAsync(
//           int categoryId, decimal? hourlyRate, decimal? dailyRate, CancellationToken ct)
//        {
//            var hourly = hourlyRate.HasValue
//                ? await AnalyzeHourlyAsync(categoryId, hourlyRate.Value, ct)
//                : null;

//            var daily = dailyRate.HasValue
//                ? await AnalyzeDailyAsync(categoryId, dailyRate.Value, ct)
//                : null;

//            return new(hourly, daily);
//        }

//        public async Task<PriceAnomalyDetail> AnalyzeHourlyAsync(int categoryId, decimal rate, CancellationToken ct)
//        {
//            var median = await GetMedianAsync(categoryId, useHourly: true, ct);
//            return BuildDetail(rate, median, "hourly");
//        }

//        public async Task<PriceAnomalyDetail> AnalyzeDailyAsync(int categoryId, decimal rate, CancellationToken ct)
//        {
//            var median = await GetMedianAsync(categoryId, useHourly: false, ct);
//            return BuildDetail(rate, median, "daily");
//        }

//        private async Task<decimal> GetMedianAsync(int categoryId, bool useHourly, CancellationToken ct)
//        {
//            var prices = useHourly
//                ? await context.ServiceListings
//                    .Where(l => l.CategoryId == categoryId
//                             && l.Status == ServiceListingStatus.Active
//                             && l.HourlyRate.HasValue)
//                    .Select(l => l.HourlyRate!.Value)
//                    .OrderBy(p => p)
//                    .ToListAsync(ct)
//                : await context.ServiceListings
//                    .Where(l => l.CategoryId == categoryId
//                             && l.Status == ServiceListingStatus.Active
//                             && l.DailyRate.HasValue)
//                    .Select(l => l.DailyRate!.Value)
//                    .OrderBy(p => p)
//                    .ToListAsync(ct);

//            if (prices.Count == 0) return 0;
//            var mid = prices.Count / 2;
//            return prices.Count % 2 == 0
//                ? (prices[mid - 1] + prices[mid]) / 2m
//                : prices[mid];
//        }

//        private static PriceAnomalyDetail BuildDetail(decimal rate, decimal median, string label)
//        {
//            if (median == 0) return new(false, rate, 0, 0, null);
//            var pct = rate / median;
//            var anomalous = pct < LowPriceThresholdPct || pct > HighPriceThresholdPct;
//            var reason = !anomalous ? null
//                : pct < LowPriceThresholdPct
//                    ? $"{label} rate is {pct:P0} of category median — suspiciously low"
//                    : $"{label} rate is {pct:P0} of category median — suspiciously high";
//            return new(anomalous, rate, median, pct, reason);
//        }
//    }
//}
