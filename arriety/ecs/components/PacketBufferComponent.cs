using System.Collections.Concurrent;

namespace arriety.ecs.components
{
    public class PacketBufferComponent : IComponent
    {
        public int EntityId { get; set; }
        
        // Receive buffer management
        public byte[] ReceiveBuffer { get; set; } = new byte[8192];
        public int ReceiveBufferLength { get; set; }
        
        // Queues for threading
        public ConcurrentQueue<byte[]> SendQueue { get; set; } = new();
        public ConcurrentQueue<ReceivedPacketData> ReceiveQueue { get; set; } = new();
        
        // Threading flags
        public volatile bool ReceiveThreadRunning;
        public volatile bool SendThreadRunning;
    }
    
    public struct ReceivedPacketData
    {
        public byte OpCode { get; set; }
        public byte[] Payload { get; set; }
    }
} 