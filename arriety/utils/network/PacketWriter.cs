using System;
using System.IO;
using System.Text;

namespace arriety.utils.network
{
    public class PacketWriter : IDisposable
    {
        private MemoryStream stream;
        private bool bigEndian;

        public PacketWriter(int capacity = 256, bool bigEndian = false)
        {
            stream = new MemoryStream(capacity);
            this.bigEndian = bigEndian;
        }

        public void WriteByte(byte value) => stream.WriteByte(value);

        public void WriteSByte(sbyte value) => stream.WriteByte((byte)value);

        public void WriteBool(bool value) => WriteByte((byte)(value ? 1 : 0));

        public void WriteShort(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (bigEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 2);
        }

        public void WriteUShort(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (bigEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 2);
        }

        public void WriteInt(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (bigEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 4);
        }

        public void WriteLong(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (bigEndian) Array.Reverse(bytes);
            stream.Write(bytes, 0, 8);
        }

        public void WriteBytes(byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }

        public void WriteUTF(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteShort(0);
                return;
            }

            var utf8 = Encoding.UTF8.GetBytes(value);
            if (utf8.Length > short.MaxValue) throw new Exception("String too long");
            WriteShort((short)utf8.Length);
            WriteBytes(utf8);
        }

        public byte[] ToArray() => stream.ToArray();

        public void Dispose() => stream.Dispose();
    }
}