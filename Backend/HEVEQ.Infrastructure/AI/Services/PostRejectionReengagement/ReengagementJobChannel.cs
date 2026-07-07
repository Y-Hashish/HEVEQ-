using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace HEVEQ.Infrastructure.AI.Services.PostRejectionReengagement
{
    public sealed class ReengagementJobChannel
    {
        internal Channel<Guid> Value { get; } = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }
}
