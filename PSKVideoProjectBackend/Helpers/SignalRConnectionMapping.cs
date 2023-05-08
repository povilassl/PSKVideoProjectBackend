namespace PSKVideoProjectBackend.Helpers
{
    public class SignalRConnectionMapping
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new();

        public int Count => _connections.Count;

        /// <summary>
        /// Adds a new connection to our dictionary
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        public void Add(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (!_connections.TryGetValue(userId, out var connections))
                {
                    connections = new HashSet<string>();
                    _connections[userId] = connections;
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        /// <summary>
        /// Removes a connection by connection Id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="connectionId"></param>
        public void Remove(string userId, string connectionId)
        {
            lock (_connections)
            {
                if (_connections.TryGetValue(userId, out var connections))
                {
                    lock (connections)
                    {
                        connections.Remove(connectionId);

                        if (connections.Count == 0)
                        {
                            _connections.Remove(userId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets all connections for user Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUserConnections(string userId)
        {
            if (_connections.TryGetValue(userId, out var connections))
            {
                return connections;
            }

            return new HashSet<string>();
        }
    }
}