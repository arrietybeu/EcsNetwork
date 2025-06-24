using arriety.utils.network;

namespace arriety.login.network.packets
{
    public class CM_AuthGG_Ecs : ClientPacket
    {
        public override byte OpCode => 0x10;
        
        public int SessionId { get; set; }
        public string Platform { get; set; } = "";
        public int MemorySizeMB { get; set; }
        public string DeviceName { get; set; } = "";

        public override void Write(PacketWriter writer)
        {
            writer.WriteInt(SessionId);
            writer.WriteUTF(Platform);
            writer.WriteInt(MemorySizeMB);
            writer.WriteUTF(DeviceName);
        }
    }
} 