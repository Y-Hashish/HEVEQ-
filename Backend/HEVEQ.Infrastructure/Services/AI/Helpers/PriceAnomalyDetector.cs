//using HEVEQ.Application.Common.AI.Models;
//using HEVEQ.Application.Common.Interfaces;
//using HEVEQ.Domain.Enums;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace HEVEQ.Infrastructure.Services.AI.Helpers
//{
//    public class PriceAnomalyDetector(IApplicationDbContext context)
//    {
//        private const decimal LowPriceThresholdPct = 0.40m;
//        private const decimal HighPriceThresholdPct = 2.00m;

//        public async Task<PriceAnomalyDetail> AnalyzeAsync(int categoryId, decimal price, CancellationToken ct)
//        {
//            var median = await GetCategoryMedianPriceAsync(categoryId, ct);
//            return BuildDetail(price, median);
//        }

//        private async Task<decimal> GetCategoryMedianPriceAsync(int categoryId, CancellationToken ct)
//        {
//            var prices = await context.MarketplaceListings
//                .Where(l => l.CategoryId == categoryId && l.Status == MarketplaceListingStatus.Active)
//                .Select(l => l.Price)
//                .OrderBy(p => p)
//                .ToListAsync(ct);

//            if (prices.Count == 0) return 0;
//            var mid = prices.Count / 2;
//            return prices.Count % 2 == 0
//                ? (prices[mid - 1] + prices[mid]) / 2m
//                : prices[mid];
//        }
        

//        private static PriceAnomalyDetail BuildDetail(decimal price, decimal median)
//        {
//            if (median == 0) return new(false, price, 0, 0, null);
//            var pct = price / median;
//            var anomalous = pct < LowPriceThresholdPct || pct > HighPriceThresholdPct;
//            var reason = !anomalous ? null
//                : pct < LowPriceThresholdPct
//                    ? $"Price is {pct:P0} of category median — suspiciously low"
//                    : $"Price is {pct:P0} of category median — suspiciously high";
//            return new(anomalous, price, median, pct, reason);
//        }
//    }
//}
