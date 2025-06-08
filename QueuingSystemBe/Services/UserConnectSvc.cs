using System.Collections.Concurrent;

namespace QueuingSystemBe.Services
{
    public class UserConnectSvc
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public void AddOrUpdateConnection(string email, string connectionId)
        {
            _connections.AddOrUpdate(email, connectionId, (key, oldValue) => connectionId);
        }

        public bool TryGetConnection(string email, out string? connectionId)
        {
            return _connections.TryGetValue(email, out connectionId);
        }

        public bool RemoveConnection(string email)
        {
            return _connections.TryRemove(email, out _);
        }

        public IReadOnlyDictionary<string, string> GetAllConnections() => _connections;
    }
}
