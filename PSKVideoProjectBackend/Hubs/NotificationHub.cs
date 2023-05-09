using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PSKVideoProjectBackend.Helpers;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PSKVideoProjectBackend.Hubs
{
    public class NotificationHub : Hub
    {

        private readonly SignalRConnectionMapping _connectionMapping;

        public NotificationHub(SignalRConnectionMapping connectionMapping)
        {
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// If a user is authenticated, adds this connection to our dictionary
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if (user is not null && user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.Name)?.Value;

                if (userId is not null) _connectionMapping.Add(userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// If a user is authenticated, removes this connection from our dictionary
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User;

            if (user is not null && user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.Name)?.Value;

                if (userId is not null) _connectionMapping.Remove(userId, Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
