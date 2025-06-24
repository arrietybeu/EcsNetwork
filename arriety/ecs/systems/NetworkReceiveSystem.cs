using arriety.ecs.components;
using arriety.utils;

namespace arriety.ecs.systems
{
    public class NetworkReceiveSystem : ISystem
    {
        private Thread? receiveThread;
        private volatile bool running;

        public void Initialize(World world)
        {
            running = true;
            receiveThread = new Thread(() => ReceiveLoop(world)) { IsBackground = true };
            receiveThread.Start();
            Log.Info("[ECS] NetworkReceiveSystem initialized");
        }

        public void Update(World world, float deltaTime)
        {
            // The actual receive work is done in the background thread
            // This update method can be used for cleanup or health checks
        }

        private void ReceiveLoop(World world)
        {
            var tmp = new byte[4096];
            
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
                            
                        buffer.ReceiveThreadRunning = true;
                        
                        if (connection.Stream.DataAvailable)
                        {
                            var count = connection.Stream.Read(tmp, 0, tmp.Length);
                            if (count <= 0)
                            {
                                // Connection lost
                                connection.IsConnected = false;
                                buffer.ReceiveThreadRunning = false;
                                continue;
                            }

                            // Expand buffer if needed
                            if (buffer.ReceiveBufferLength + count > buffer.ReceiveBuffer.Length)
                            {
                                ExpandBuffer(buffer, buffer.ReceiveBufferLength + count);
                            }

                            // Copy data to buffer
                            Array.Copy(tmp, 0, buffer.ReceiveBuffer, buffer.ReceiveBufferLength, count);
                            buffer.ReceiveBufferLength += count;

                            // Process the buffer
                            ProcessReceiveBuffer(buffer);
                        }
                    }
                    
                    Thread.Sleep(1); // Small delay to prevent tight loop
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    Thread.Sleep(100); // Longer delay on error
                }
            }
        }

        private void ProcessReceiveBuffer(PacketBufferComponent buffer)
        {
            var offset = 0;
            
            while (buffer.ReceiveBufferLength - offset >= 3)
            {
                var length = (ushort)(buffer.ReceiveBuffer[offset] | (buffer.ReceiveBuffer[offset + 1] << 8));

                if (length < 3)
                {
                    Log.Warning($"[ECS] Invalid packet length: {length}");
                    return;
                }

                if (buffer.ReceiveBufferLength - offset < length)
                    break; // Need more data

                var opcode = buffer.ReceiveBuffer[offset + 2];
                var payloadLen = length - 3;
                var payload = new byte[payloadLen];
                Array.Copy(buffer.ReceiveBuffer, offset + 3, payload, 0, payloadLen);

                Log.Info($"[ECS] Received opcode 0x{opcode:X2}, length: {length}");

                // Queue the packet for processing
                buffer.ReceiveQueue.Enqueue(new ReceivedPacketData
                {
                    OpCode = opcode,
                    Payload = payload
                });

                offset += length;
            }

            // Compact the buffer
            if (offset > 0)
            {
                Array.Copy(buffer.ReceiveBuffer, offset, buffer.ReceiveBuffer, 0, buffer.ReceiveBufferLength - offset);
                buffer.ReceiveBufferLength -= offset;
            }
        }

        private void ExpandBuffer(PacketBufferComponent buffer, int minSize)
        {
            var newSize = buffer.ReceiveBuffer.Length * 2;
            while (newSize < minSize) newSize *= 2;

            var newBuffer = new byte[newSize];
            Array.Copy(buffer.ReceiveBuffer, 0, newBuffer, 0, buffer.ReceiveBufferLength);
            buffer.ReceiveBuffer = newBuffer;
        }

        public void Shutdown(World world)
        {
            running = false;
            receiveThread?.Join(1000); // Wait up to 1 second for thread to finish
            Log.Info("[ECS] NetworkReceiveSystem shutdown");
        }
    }
} 