using HEVEQ.Application.Common.Realtime;
using HEVEQ.Application.Features.Notifications.DTOs;

namespace HEVEQ.Application.Common.Interfaces
{
    public interface IRealtimeEventPublisher
    {
        Task SendNotificationToUserAsync(Guid userId, NotificationItemDto notification, CancellationToken cancellationToken = default);
        Task SendMessageToUserAsync(Guid userId, RealtimeMessageDto message, CancellationToken cancellationToken = default);
    }
}