using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network.packets
{
    public class CM_AuthGG : ClientPacket
    {
        public override byte OpCode => 0x10;

        public override void Write(PacketWriter writer)
        {
            var network = LoginManager.Instance.network;
            writer.WriteInt(network.SessionId);
            writer.WriteUTF(DeviceInfo.Platform);
            writer.WriteInt(DeviceInfo.MemorySizeMB);
            writer.WriteUTF(DeviceInfo.DeviceName);
        }
    }
}