using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using QueuingSystemBe.Services;

namespace QueuingSystemBe.HubForRealTime
{
    public class AuthHub : Hub
    {
        private readonly UserConnectSvc _connectionManager;

        public AuthHub(UserConnectSvc connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task Login(string email)
        {
            if (_connectionManager.TryGetConnection(email, out var oldConnectionId))
            {
                // Gửi lệnh đăng xuất tới thiết bị cũ
                await Clients.Client(oldConnectionId).SendAsync("ForceLogout");
            }
            _connectionManager.AddOrUpdateConnection(email, Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            // Khi client disconnect, loại bỏ connection
            foreach (var entry in _connectionManager.GetAllConnections())
            {
                if (entry.Value == Context.ConnectionId)
                {
                    _connectionManager.RemoveConnection(entry.Key);
                    break;
                }
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
