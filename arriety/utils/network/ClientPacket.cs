namespace arriety.utils.network
{
    public abstract class ClientPacket
    {
        public abstract byte OpCode { get; }
        public abstract void Write(PacketWriter writer);
    }
}