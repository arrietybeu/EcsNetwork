using arriety.ecs.components;
using arriety.ecs.systems;
using arriety.utils;

namespace arriety.ecs
{
    public class EcsNetworkManager : IDisposable
    {
        private World world;
        private Entity networkEntity;
        private Thread updateThread;
        private volatile bool running;
        private readonly object disposeLock = new();

        public event Action? OnDisconnected;
        public event Action? OnConnected;
        public event Action? OnLoginSuccess;
        public event Action<string>? OnLoginFailed;

        public EcsNetworkManager()
        {
            world = new World();
            InitializeSystems();
            InitializeNetworkEntity();
        }

        private void InitializeSystems()
        {
            // Add all networking systems
            world.AddSystem(new ConnectionSystem());
            world.AddSystem(new NetworkReceiveSystem());
            world.AddSystem(new NetworkSendSystem());
            world.AddSystem(new PacketDispatchSystem());
            world.AddSystem(new LoginSystem());
        }

        private void InitializeNetworkEntity()
        {
            // Create the main network entity with all necessary components
            networkEntity = world.CreateEntity();
            
            // Add connection component
            var connection = networkEntity.AddComponent<NetworkConnectionComponent>();
            
            // Add packet buffer component
            networkEntity.AddComponent<PacketBufferComponent>();
            
            // Add session component
            networkEntity.AddComponent<SessionComponent>();
            
            // Add login state component
            var loginState = networkEntity.AddComponent<LoginStateComponent>();
            loginState.State = LoginState.Disconnected;
            loginState.LastStateChange = DateTime.Now;
            
            // Add device info component
            var deviceInfo = networkEntity.AddComponent<DeviceInfoComponent>();
            deviceInfo.InitializeFromEnvironment();
        }

        public void Connect(string host, int port)
        {
            var connection = networkEntity.GetComponent<NetworkConnectionComponent>()!;
            var loginState = networkEntity.GetComponent<LoginStateComponent>()!;
            
            connection.Host = host;
            connection.Port = port;
            connection.ShouldReconnect = true;
            connection.ReconnectAttempts = 0;
            
            loginState.State = LoginState.Connecting;
            loginState.LastStateChange = DateTime.Now;
            loginState.AuthSent = false;
            
            StartUpdateLoop();
            
            Log.Info($"[ECS] Connecting to {host}:{port}");
        }

        private void StartUpdateLoop()
        {
            if (running) return;
            
            running = true;
            world.Initialize();
            
            updateThread = new Thread(UpdateLoop) { IsBackground = true };
            updateThread.Start();
        }

        private void UpdateLoop()
        {
            var lastTime = DateTime.Now;
            
            while (running)
            {
                try
                {
                    var currentTime = DateTime.Now;
                    var deltaTime = (float)(currentTime - lastTime).TotalSeconds;
                    lastTime = currentTime;
                    
                    world.Update(deltaTime);
                    
                    // Check for state changes and fire events
                    CheckForStateChanges();
                    
                    Thread.Sleep(16); // ~60 FPS update rate
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    Thread.Sleep(100); // Longer delay on error
                }
            }
        }

        private bool wasConnected = false;
        private LoginState lastLoginState = LoginState.Disconnected;
        
        private void CheckForStateChanges()
        {
            var connection = networkEntity.GetComponent<NetworkConnectionComponent>()!;
            var loginState = networkEntity.GetComponent<LoginStateComponent>()!;
            
            // Check connection status
            if (connection.IsConnected && !wasConnected)
            {
                wasConnected = true;
                OnConnected?.Invoke();
            }
            else if (!connection.IsConnected && wasConnected)
            {
                wasConnected = false;
                OnDisconnected?.Invoke();
            }
            
            // Check login status
            if (loginState.State != lastLoginState)
            {
                switch (loginState.State)
                {
                    case LoginState.Authenticated:
                        OnLoginSuccess?.Invoke();
                        break;
                    case LoginState.LoginFailed:
                        OnLoginFailed?.Invoke(loginState.LoginFailMessage ?? "Unknown error");
                        break;
                }
                lastLoginState = loginState.State;
            }
        }

        public LoginState GetLoginState()
        {
            return networkEntity.GetComponent<LoginStateComponent>()?.State ?? LoginState.Disconnected;
        }

        public bool IsConnected()
        {
            return networkEntity.GetComponent<NetworkConnectionComponent>()?.IsConnected ?? false;
        }

        public int GetSessionId()
        {
            return networkEntity.GetComponent<SessionComponent>()?.SessionId ?? 0;
        }

        public void Disconnect()
        {
            lock (disposeLock)
            {
                if (!running) return;
                
                running = false;
                
                var connection = networkEntity.GetComponent<NetworkConnectionComponent>()!;
                connection.ShouldReconnect = false;
                connection.IsConnected = false;
                
                world.Shutdown();
                updateThread?.Join(2000);
                
                Log.Info("[ECS] Disconnected");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
} 