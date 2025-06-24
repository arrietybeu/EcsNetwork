using arriety.login.network.packets;
using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network.factories
{
    public class SM_INIT : ServerPacket
    {
        private readonly NetworkManager network = LoginManager.Instance.network;

        public override void Read(PacketReader reader)
        {
            network.SessionId = reader.ReadInt();
        }

        public override void Run()
        {
            Log.Info($"[Network] Session initialized: {network.SessionId}");

            network.SendPacket(new CM_AuthGG());
        }
    }
}