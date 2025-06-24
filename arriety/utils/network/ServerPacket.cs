namespace arriety.utils.network
{
    public abstract class ServerPacket
    {
        public abstract void Read(PacketReader reader);

        public abstract void Run();
    }
}