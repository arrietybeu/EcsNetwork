using System.Net.Sockets;

namespace arriety.ecs.components
{
    public class NetworkConnectionComponent : IComponent
    {
        public int EntityId { get; set; }
        
        public TcpClient? Client { get; set; }
        public NetworkStream? Stream { get; set; }
        public string Host { get; set; } = "";
        public int Port { get; set; }
        public bool IsConnected { get; set; }
        public bool IsConnecting { get; set; }
        public bool ShouldReconnect { get; set; }
        public DateTime LastConnectionAttempt { get; set; }
        public int ReconnectAttempts { get; set; }
        public int MaxReconnectAttempts { get; set; } = 5;
    }
} 