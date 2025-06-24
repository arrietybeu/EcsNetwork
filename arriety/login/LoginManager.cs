using arriety.ecs;
using arriety.ecs.components;
using arriety.login.network;

namespace arriety.login
{
    public class LoginManager
    {
        // Legacy property for backward compatibility
        public NetworkManager? network { get; private set; }
        
        // New ECS-based network manager
        public EcsNetworkManager EcsNetwork { get; private set; }

        public static LoginManager Instance { get; private set; }

        public LoginManager()
        {
            Instance = this;
            
            // Initialize ECS network manager
            EcsNetwork = new EcsNetworkManager();
            
            // Set up event handlers
            EcsNetwork.OnConnected += HandleConnect;
            EcsNetwork.OnDisconnected += HandleDisconnect;
            EcsNetwork.OnLoginSuccess += HandleLoginSuccess;
            EcsNetwork.OnLoginFailed += HandleLoginFailed;

            // Connect using ECS system
            EcsNetwork.Connect("127.0.0.1", 1906);
        }

        private void OnDestroy()
        {
            EcsNetwork?.Dispose();
            if (Instance == this) Instance = null;
        }

        private static void HandleConnect()
        {
            Console.WriteLine("[ECS] Connected to login server.");
        }

        private static void HandleDisconnect()
        {
            Console.WriteLine("[ECS] Lost connection to login server.");
        }
        
        private static void HandleLoginSuccess()
        {
            Console.WriteLine("[ECS] Login successful!");
        }
        
        private static void HandleLoginFailed(string message)
        {
            Console.WriteLine($"[ECS] Login failed: {message}");
        }
        
        // Backward compatibility methods
        public bool IsConnected => EcsNetwork?.IsConnected() ?? false;
        public LoginState GetLoginState() => EcsNetwork?.GetLoginState() ?? LoginState.Disconnected;
        public int GetSessionId() => EcsNetwork?.GetSessionId() ?? 0;
    }
}