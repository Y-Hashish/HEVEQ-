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
    public class EscrowReleaseAfterCompletionJob
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<EscrowReleaseAfterCompletionJob> _logger;
        private readonly BackgroundJobOptions _options;
        private readonly NotificationHelper _notificationHelper;

        public EscrowReleaseAfterCompletionJob(IApplicationDbContext context, ILogger<EscrowReleaseAfterCompletionJob> logger, IOptions<BackgroundJobOptions> options, NotificationHelper notificationHelper)
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
            var completionCutoff = now.AddHours(-_options.EscrowReleaseAfterCompletionHours);

            var bookings = await _context.Bookings
                .Include(x => x.EscrowRecords)
                .Include(x => x.ServiceListing)
                    .ThenInclude(x => x.ProviderProfile)
                .Where(x =>
                    x.Status == BookingStatus.Completed &&
                    x.CompletionConfirmedAt != null &&
                    x.CompletionConfirmedAt <= completionCutoff &&
                    x.DisputeOpenedAt == null)
                .ToListAsync();

            if (bookings.Count == 0)
            {
                _logger.LogInformation("EscrowReleaseAfterCompletionJob: no eligible bookings found.");
                return;
            }

            var releasedCount = 0;

            foreach (var booking in bookings)
            {
                var escrow = booking.EscrowRecords
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();

                if (escrow is null)
                    continue;

                if (escrow.Status != EscrowStatus.Held)
                    continue;

                if (escrow.FrozenAt is not null)
                    continue;

                if (escrow.EarliestReleaseAt is not null &&
                    escrow.EarliestReleaseAt > now)
                {
                    continue;
                }

                escrow.Status = EscrowStatus.Released;
                escrow.ReleasedAt = now;

                releasedCount++;

                _notificationHelper.EscrowReleased(booking.ServiceListing.ProviderProfile.UserId,escrow.Id,booking.BookingNumber);
            }

            if (releasedCount == 0)
            {
                _logger.LogInformation("EscrowReleaseAfterCompletionJob: no escrow records released.");
                return;
            }

            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("EscrowReleaseAfterCompletionJob: released {Count} escrow records.", releasedCount);
        }
    }
}