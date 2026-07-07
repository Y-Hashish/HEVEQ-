using HEVEQ.Application.Common.AI.Interfaces.ReEngagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Infrastructure.AI.Services.PostRejectionReengagement
{
    public sealed class ChannelReengagementScheduler : IReengagementJobScheduler
    {
        private readonly ReengagementJobChannel _channel;

        public ChannelReengagementScheduler(ReengagementJobChannel channel)
            => _channel = channel;

        public Task ScheduleAsync(Guid rejectedBookingId, CancellationToken ct = default)
        {
            _channel.Value.Writer.TryWrite(rejectedBookingId);
            return Task.CompletedTask;
        }
    }
}
