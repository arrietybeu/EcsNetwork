using System.Net.Sockets;
using arriety.ecs.components;
using arriety.utils;

namespace arriety.ecs.systems
{
    public class ConnectionSystem : ISystem
    {
        public void Initialize(World world)
        {
            Log.Info("[ECS] ConnectionSystem initialized");
        }

        public void Update(World world, float deltaTime)
        {
            var entities = world.GetEntitiesWith<NetworkConnectionComponent, LoginStateComponent>();
            
            foreach (var entity in entities)
            {
                var connection = entity.GetComponent<NetworkConnectionComponent>()!;
                var loginState = entity.GetComponent<LoginStateComponent>()!;
                
                // Handle connection logic
                if (!connection.IsConnected && !connection.IsConnecting && connection.ShouldReconnect)
                {
                    TryConnect(connection, loginState);
                }
                
                // Check connection health
                if (connection.IsConnected && connection.Client?.Connected == false)
                {
                    HandleDisconnection(connection, loginState);
                }
            }
        }
        
        private void TryConnect(NetworkConnectionComponent connection, LoginStateComponent loginState)
        {
            if (connection.ReconnectAttempts >= connection.MaxReconnectAttempts)
            {
                Log.Error($"[ECS] Max reconnection attempts reached for {connection.Host}:{connection.Port}");
                connection.ShouldReconnect = false;
                return;
            }
            
            try
            {
                connection.IsConnecting = true;
                loginState.State = LoginState.Connecting;
                loginState.LastStateChange = DateTime.Now;
                
                connection.Client = new TcpClient();
                connection.Client.Connect(connection.Host, connection.Port);
                connection.Stream = connection.Client.GetStream();
                
                connection.IsConnected = true;
                connection.IsConnecting = false;
                connection.ReconnectAttempts = 0;
                
                loginState.State = LoginState.WaitingForInit;
                loginState.LastStateChange = DateTime.Now;
                
                Log.Info($"[ECS] Connected to {connection.Host}:{connection.Port}");
            }
            catch (Exception e)
            {
                connection.IsConnecting = false;
                connection.ReconnectAttempts++;
                connection.LastConnectionAttempt = DateTime.Now;
                
                Log.Exception(e);
                Log.Warning($"[ECS] Connection attempt {connection.ReconnectAttempts} failed");
            }
        }
        
        private void HandleDisconnection(NetworkConnectionComponent connection, LoginStateComponent loginState)
        {
            Log.Warning("[ECS] Connection lost");
            
            connection.IsConnected = false;
            connection.Stream?.Close();
            connection.Client?.Close();
            connection.Stream = null;
            connection.Client = null;
            
            loginState.State = LoginState.Disconnected;
            loginState.LastStateChange = DateTime.Now;
            
            // Set reconnection flag if needed
            connection.ShouldReconnect = true;
        }

        public void Shutdown(World world)
        {
            var entities = world.GetEntitiesWith<NetworkConnectionComponent>();
            
            foreach (var entity in entities)
            {
                var connection = entity.GetComponent<NetworkConnectionComponent>()!;
                connection.Stream?.Close();
                connection.Client?.Close();
            }
            
            Log.Info("[ECS] ConnectionSystem shutdown");
        }
    }
} 