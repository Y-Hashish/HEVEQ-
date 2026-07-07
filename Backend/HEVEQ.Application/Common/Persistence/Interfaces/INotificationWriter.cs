using System;
using System.Collections.Generic;
using System.Text;

namespace HEVEQ.Application.Common.Persistence.Interfaces
{
    /// <summary>
    /// Writes a row to the Notifications table (Channel = 0/InApp and 1/Push).
    /// </summary>
    public interface INotificationWriter
    {
        Task SendAsync(
            Guid userId,
            string eventType,
            string title,
            string body,
            string? referenceId,
            string? referenceType,
            CancellationToken ct = default);
    }
}
