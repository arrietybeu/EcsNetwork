using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network.factories
{
    public class SM_LOGIN_RESPONSE : ServerPacket
    {
        private string message;

        public override void Read(PacketReader reader)
        {
            var type = reader.ReadByte();
            switch (type)
            {
                case 0: // OK
                    break;
                case 1: // FAIL
                    message = reader.ReadUTF();
                    break;
            }
        }

        public override void Run()
        {
            if (message == null) return;
            LoginManager.Instance.network.Dispose();
            Log.Info(message);
        }
    }
}