using arriety.ecs.components;
using arriety.login.network;
using arriety.utils;
using arriety.utils.network;

namespace arriety.ecs.systems
{
    public class PacketDispatchSystem : ISystem
    {
        public void Initialize(World world)
        {
            Log.Info("[ECS] PacketDispatchSystem initialized");
        }

        public void Update(World world, float deltaTime)
        {
            var entities = world.GetEntitiesWith<PacketBufferComponent>();
            
            foreach (var entity in entities)
            {
                var buffer = entity.GetComponent<PacketBufferComponent>()!;
                
                // Process all queued packets
                while (buffer.ReceiveQueue.TryDequeue(out var packetData))
                {
                    ProcessPacket(entity, packetData);
                }
            }
        }
        
        private void ProcessPacket(Entity entity, ReceivedPacketData packetData)
        {
            var packet = ServerPacketFactory.Create(packetData.OpCode);
            if (packet == null)
            {
                Log.Warning($"[ECS] Unknown opcode: 0x{packetData.OpCode:X2}");
                return;
            }

            try
            {
                using var reader = new PacketReader(packetData.Payload);
                
                // Handle packet based on type
                if (packet is IEcsPacket ecsPacket)
                {
                    // New ECS-compatible packet
                    ecsPacket.ProcessInEcs(entity, reader);
                }
                else
                {
                    // Legacy packet handling
                    packet.Read(reader);
                    packet.Run();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                Log.Error($"[ECS] Error processing packet 0x{packetData.OpCode:X2}");
            }
        }

        public void Shutdown(World world)
        {
            Log.Info("[ECS] PacketDispatchSystem shutdown");
        }
    }
    
    // Interface for ECS-compatible packets
    public interface IEcsPacket
    {
        void ProcessInEcs(Entity entity, PacketReader reader);
    }
} 