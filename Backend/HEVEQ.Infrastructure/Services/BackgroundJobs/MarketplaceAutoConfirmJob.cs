using Hangfire;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Jobs;
using HEVEQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HEVEQ.Application.Common.Services;

namespace HEVEQ.Infrastructure.Services.BackgroundJobs
{
    public class MarketplaceAutoConfirmJob
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<MarketplaceAutoConfirmJob> _logger;
        private readonly BackgroundJobOptions _options;
        private readonly NotificationHelper _notificationHelper;

        public MarketplaceAutoConfirmJob(IApplicationDbContext context, ILogger<MarketplaceAutoConfirmJob> logger, IOptions<BackgroundJobOptions> options, NotificationHelper notificationHelper)
        {
            _context = context;
            _logger = logger;
            _options = options.Value;
            _notificationHelper = notificationHelper;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        public async Task RunAsync()
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddHours(-_options.MarketplaceAutoConfirmHours);

            var orders = await _context.MarketplaceOrders
                .Include(x => x.Listing)
                .Where(x =>
                    x.Status == MarketplaceOrderStatus.Delivered &&
                    x.DeliveredAt != null &&
                    x.DeliveredAt <= cutoff)
                .ToListAsync();

            if (orders.Count == 0)
            {
                _logger.LogInformation("MarketplaceAutoConfirmJob: no delivered orders eligible for auto-confirm.");
                return;
            }

            foreach (var order in orders)
            {
                order.Status = MarketplaceOrderStatus.Completed;
                order.ConfirmedByBuyerAt = now;

                _notificationHelper.MarketplaceOrderAutoCompleted(order.Listing.SellerId,order.Id,order.OrderNumber);
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation(
                "MarketplaceAutoConfirmJob: auto-confirmed {Count} marketplace orders.",
                orders.Count);
        }
    }
}