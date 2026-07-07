using HEVEQ.Application.Features.Bookings.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.AI.Services.PostRejectionReengagement
{
    public sealed class ReengagementBackgroundWorker : BackgroundService
    {
        private static readonly TimeSpan DelayBeforeProcessing = TimeSpan.FromSeconds(30);

        private readonly ReengagementJobChannel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReengagementBackgroundWorker> _logger;

        public ReengagementBackgroundWorker(
            ReengagementJobChannel channel,
            IServiceScopeFactory scopeFactory,
            ILogger<ReengagementBackgroundWorker> logger)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var bookingId in _channel.Value.Reader.ReadAllAsync(stoppingToken))
            {
                _ = ProcessJobAsync(bookingId, stoppingToken);
            }
        }

        private async Task ProcessJobAsync(Guid bookingId, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(DelayBeforeProcessing, stoppingToken);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var executor = scope.ServiceProvider.GetRequiredService<ReengagementJobExecutor>();

                await executor.ExecuteAsync(bookingId, stoppingToken);

                _logger.LogInformation("Re-engagement completed for rejected booking {BookingId}.", bookingId);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Re-engagement job failed for booking {BookingId}.", bookingId);
            }
        }
    }
}
