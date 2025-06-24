using arriety.ecs;
using arriety.ecs.components;
using arriety.ecs.systems;
using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network.factories
{
    public class SM_INIT_Ecs : ServerPacket, IEcsPacket
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
            var session = entity.GetComponent<SessionComponent>();
            if (session == null) return;
            
            session.SessionId = reader.ReadInt();
            session.IsSessionInitialized = true;
            session.SessionStartTime = DateTime.Now;
            
            Log.Info($"[ECS] Session initialized: {session.SessionId}");
        }
    }
} 