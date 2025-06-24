using arriety.ecs.components;
using arriety.login.network.packets;
using arriety.utils;

namespace arriety.ecs.systems
{
    public class LoginSystem : ISystem
    {
        public void Initialize(World world)
        {
            Log.Info("[ECS] LoginSystem initialized");
        }

        public void Update(World world, float deltaTime)
        {
            var entities = world.GetEntitiesWith<LoginStateComponent, SessionComponent>();
            
            foreach (var entity in entities)
            {
                var loginState = entity.GetComponent<LoginStateComponent>()!;
                var session = entity.GetComponent<SessionComponent>()!;
                
                // Handle auto-authentication after session init
                if (loginState.State == LoginState.WaitingForInit && 
                    session.IsSessionInitialized && 
                    !loginState.AuthSent)
                {
                    Log.Info("[ECS] Sending authentication packet");
                    SendAuthPacket(entity);
                    loginState.AuthSent = true;
                    loginState.State = LoginState.Authenticating;
                    loginState.LastStateChange = DateTime.Now;
                }
                
                // Handle login timeout
                if (loginState.State == LoginState.Authenticating && 
                    DateTime.Now - loginState.LastStateChange > TimeSpan.FromSeconds(30))
                {
                    Log.Warning("[ECS] Login timeout");
                    loginState.State = LoginState.LoginFailed;
                    loginState.LoginFailMessage = "Login timeout";
                    loginState.LastStateChange = DateTime.Now;
                }
            }
        }
        
        private void SendAuthPacket(Entity entity)
        {
            var deviceInfo = entity.GetComponent<DeviceInfoComponent>();
            var session = entity.GetComponent<SessionComponent>();
            
            if (deviceInfo == null || session == null) return;
            
            var authPacket = new CM_AuthGG_Ecs
            {
                SessionId = session.SessionId,
                Platform = deviceInfo.Platform,
                MemorySizeMB = deviceInfo.MemorySizeMB,
                DeviceName = deviceInfo.DeviceName
            };
            
            NetworkSendSystem.SendPacket(entity, authPacket);
        }

        public void Shutdown(World world)
        {
            Log.Info("[ECS] LoginSystem shutdown");
        }
    }
} 