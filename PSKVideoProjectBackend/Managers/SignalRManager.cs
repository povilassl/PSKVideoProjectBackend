using Microsoft.AspNetCore.SignalR;
using PSKVideoProjectBackend.Helpers;
using PSKVideoProjectBackend.Hubs;

namespace PSKVideoProjectBackend.Managers
{
    public class SignalRManager
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly SignalRConnectionMapping _connectionMapping;

        private static string ReceiveNotification => "ReceiveNotification";

        /// <summary>
        /// Works like a wrapper class for pushing messages
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="connectionMapping"></param>
        public SignalRManager(IHubContext<NotificationHub> hubContext, SignalRConnectionMapping connectionMapping)
        {
            _hubContext = hubContext;
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// Sends a specified message to all clients
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageToAllClients(string message)
        {
            if (String.IsNullOrEmpty(message)) return;

            await _hubContext.Clients.All.SendAsync(ReceiveNotification, message);
        }

        /// <summary>
        /// Sends a message to all clients with given userId - user can have multiple devices (clients) connected
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageToUser(string userId, string message)
        {
            var clients = _connectionMapping.GetUserConnections(userId);

            if (clients is null || clients.Count() == 0 || string.IsNullOrEmpty(message)) return;

            await _hubContext.Clients.Clients(clients).SendAsync(ReceiveNotification, message);
        }

        /// <summary>
        /// Send a message to a single client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageToClient(string clientId, string message)
        {
            if (String.IsNullOrEmpty(clientId) || String.IsNullOrEmpty(message)) return;

            await _hubContext.Clients.Client(clientId).SendAsync(ReceiveNotification, message);
        }
    }
}
