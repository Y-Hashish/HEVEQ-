using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HEVEQ.Api.Realtime.Hubs
{
    [Authorize]
    public class RealtimeHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, BuildUserGroupName(userId.Value));
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, BuildUserGroupName(userId.Value));
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task Ping()
        {
            return Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

        public static string BuildUserGroupName(Guid userId)
        {
            return $"user:{userId}";
        }

        private Guid? GetCurrentUserId()
        {
            var userIdString =Context.User?.FindFirstValue("uid") ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }
    }
}