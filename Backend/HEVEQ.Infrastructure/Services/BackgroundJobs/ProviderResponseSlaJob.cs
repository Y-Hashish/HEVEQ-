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
    public class ProviderResponseSlaJob
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<ProviderResponseSlaJob> _logger;
        private readonly BackgroundJobOptions _options;
        private readonly NotificationHelper _notificationHelper;

        public ProviderResponseSlaJob(IApplicationDbContext context, ILogger<ProviderResponseSlaJob> logger, IOptions<BackgroundJobOptions> options, NotificationHelper notificationHelper)
        {
            _context = context;
            _logger = logger;
            _options = options.Value;
            _notificationHelper = notificationHelper;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        public async Task RunAsync()
        {
            var cutoff = DateTime.UtcNow.AddHours(-_options.ProviderResponseSlaHours);

            var expiredBookings = await _context.Bookings
                .Where(x =>
                    x.Status == BookingStatus.PendingProviderResponse &&
                    x.CreatedAt <= cutoff)
                .ToListAsync();

            if (expiredBookings.Count == 0)
            {
                _logger.LogInformation("ProviderResponseSlaJob: no expired bookings found.");
                return;
            }

            foreach (var booking in expiredBookings)
            {
                booking.Status = BookingStatus.ProviderUnresponsive;
                _notificationHelper.ProviderUnresponsive(booking.CustomerId,booking.Id, booking.BookingNumber);
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("ProviderResponseSlaJob: marked {Count} bookings as ProviderUnresponsive.", expiredBookings.Count);
        }
    }
}