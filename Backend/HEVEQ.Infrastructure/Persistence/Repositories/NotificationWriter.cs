using HEVEQ.Application.Common.Persistence.Interfaces;
using HEVEQ.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace HEVEQ.Infrastructure.Persistence.Repositories;

/// <summary>
/// Persists a notification row to the Notifications table (v3.0 schema, GAP-025)
/// and is called by ReengagementJobExecutor after the 30-second delay fires.
///
/// Channel 0 = InApp (always written).
/// The push channel (FCM via FcmToken on Users) is intentionally left to a
/// separate push-notification microservice triggered by the Notifications table —
/// not implemented here to keep the boundary clean.
/// </summary>
public sealed class NotificationWriter : INotificationWriter
{
    // ADAPT: replace with your actual DbContext class name
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationWriter> _logger;

    public NotificationWriter(
        ApplicationDbContext context,
        ILogger<NotificationWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid userId,
        string eventType,
        string title,
        string body,
        string? referenceId,
        string? referenceType,
        CancellationToken ct = default)
    {
        try
        {
            // ADAPT: adjust property names to match your Notification EF entity
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventType = eventType,
                Title = title,
                Body = body,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                IsRead = false,
                Channel = 0,              // 0 = InApp (v3.0 schema)
                SentAt = DateTime.UtcNow,
                ReadAt = null
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Notification '{EventType}' written for user {UserId}.", eventType, userId);
        }
        catch (Exception ex)
        {
            // Log but do not rethrow — a failed notification write must not abort
            // the re-engagement pipeline or cause the background worker to crash.
            _logger.LogError(ex,
                "Failed to write notification '{EventType}' for user {UserId}.",
                eventType, userId);
        }
    }
}