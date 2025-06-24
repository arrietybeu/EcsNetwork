using arriety.ecs;
using arriety.ecs.components;
using arriety.ecs.systems;
using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network.factories
{
    public class SM_LOGIN_RESPONSE_Ecs : ServerPacket, IEcsPacket
    {
        public override void Read(PacketReader reader)
        {
            // Legacy implementation for backward compatibility
        }

        public override void Run()
        {
            // Legacy implementation for backward compatibility
        }
        
        public void ProcessInEcs(Entity entity, PacketReader reader)
        {
            var loginState = entity.GetComponent<LoginStateComponent>();
            if (loginState == null) return;
            
            var type = reader.ReadByte();
            switch (type)
            {
                case 0: // OK
                    loginState.State = LoginState.Authenticated;
                    loginState.LoginFailMessage = null;
                    loginState.LastStateChange = DateTime.Now;
                    Log.Info("[ECS] Login successful");
                    break;
                    
                case 1: // FAIL
                    var message = reader.ReadUTF();
                    loginState.State = LoginState.LoginFailed;
                    loginState.LoginFailMessage = message;
                    loginState.LastStateChange = DateTime.Now;
                    Log.Info($"[ECS] Login failed: {message}");
                    
                    // Disconnect on login failure
                    var connection = entity.GetComponent<NetworkConnectionComponent>();
                    if (connection != null)
                    {
                        connection.IsConnected = false;
                        connection.ShouldReconnect = false;
                    }
                    break;
            }
        }
    }
} 