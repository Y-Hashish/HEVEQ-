using HEVEQ.Api.Realtime.Hubs;
using HEVEQ.Application.Common.Interfaces;
using HEVEQ.Application.Common.Realtime;
using HEVEQ.Application.Features.Notifications.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace HEVEQ.Api.Realtime.Services
{
    public class SignalRRealtimeEventPublisher : IRealtimeEventPublisher
    {
        private readonly IHubContext<RealtimeHub> _hubContext;
        public SignalRRealtimeEventPublisher(IHubContext<RealtimeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendNotificationToUserAsync(Guid userId, NotificationItemDto notification, CancellationToken cancellationToken = default)
        {
            var groupName = RealtimeHub.BuildUserGroupName(userId);

            return _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveNotification", notification, cancellationToken);
        }

        public Task SendMessageToUserAsync(Guid userId, RealtimeMessageDto message, CancellationToken cancellationToken = default)
        {
            var groupName = RealtimeHub.BuildUserGroupName(userId);

            return _hubContext.Clients
                .Group(groupName)
                .SendAsync("ReceiveMessage", message, cancellationToken);
        }
    }
}