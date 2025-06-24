using arriety.ecs.components;
using arriety.utils;
using arriety.utils.network;

namespace arriety.ecs.systems
{
    public class NetworkSendSystem : ISystem
    {
        private Thread? sendThread;
        private volatile bool running;

        public void Initialize(World world)
        {
            running = true;
            sendThread = new Thread(() => SendLoop(world)) { IsBackground = true };
            sendThread.Start();
            Log.Info("[ECS] NetworkSendSystem initialized");
        }

        public void Update(World world, float deltaTime)
        {
            // The actual send work is done in the background thread
            // This update method can be used for queuing packets from the main thread
        }

        private void SendLoop(World world)
        {
            while (running)
            {
                try
                {
                    var entities = world.GetEntitiesWith<NetworkConnectionComponent, PacketBufferComponent>();
                    
                    foreach (var entity in entities)
                    {
                        var connection = entity.GetComponent<NetworkConnectionComponent>()!;
                        var buffer = entity.GetComponent<PacketBufferComponent>()!;
                        
                        if (!connection.IsConnected || connection.Stream == null)
                            continue;
                            
                        buffer.SendThreadRunning = true;
                        
                        if (buffer.SendQueue.TryDequeue(out var data))
                        {
                            try
                            {
                                connection.Stream.Write(data, 0, data.Length);
                                Log.Info($"[ECS] Sent packet: {BitConverter.ToString(data)}");
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e);
                                connection.IsConnected = false;
                                buffer.SendThreadRunning = false;
                            }
                        }
                    }
                    
                    Thread.Sleep(2); // Small delay to prevent tight loop
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    Thread.Sleep(100); // Longer delay on error
                }
            }
        }

        public void Shutdown(World world)
        {
            running = false;
            sendThread?.Join(1000); // Wait up to 1 second for thread to finish
            Log.Info("[ECS] NetworkSendSystem shutdown");
        }
        
        // Static method to send packets from other systems
        public static void SendPacket(Entity entity, ClientPacket packet)
        {
            var buffer = entity.GetComponent<PacketBufferComponent>();
            if (buffer == null) return;
            
            byte[] payload;
            using (var pw = new PacketWriter())
            {
                pw.WriteByte(packet.OpCode);
                packet.Write(pw);
                payload = pw.ToArray();
            }

            SendPacket(entity, payload);
        }
        
        public static void SendPacket(Entity entity, byte[] data)
        {
            var buffer = entity.GetComponent<PacketBufferComponent>();
            if (buffer == null) return;
            
            using (var pw = new PacketWriter())
            {
                pw.WriteUShort((ushort)(data.Length + 2));
                pw.WriteBytes(data);
                buffer.SendQueue.Enqueue(pw.ToArray());
            }
        }
    }
} 