using System;
using System.IO;
using System.Text;

namespace arriety.utils.network
{
    public class PacketReader : IDisposable
    {
        private readonly MemoryStream stream;
        private readonly bool bigEndian;

        public PacketReader(byte[] data, bool bigEndian = false)
        {
            stream = new MemoryStream(data);
            this.bigEndian = bigEndian;
        }

        public byte ReadByte()
        {
            var value = stream.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (byte)value;
        }

        public sbyte ReadSByte()
        {
            var value = stream.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (sbyte)value;
        }

        public bool ReadBool() => ReadByte() != 0;

        public short ReadShort()
        {
            var bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public ushort ReadUShort()
        {
            var bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public int ReadInt()
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long ReadLong()
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public float ReadFloat()
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble()
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (bigEndian) Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte[] ReadBytes(int length)
        {
            var bytes = new byte[length];
            int read = stream.Read(bytes, 0, length);
            if (read != length) throw new EndOfStreamException();
            return bytes;
        }

        // [short length][utf8 bytes] – giống server Java writeUTF
        public string ReadUTF()
        {
            var len = ReadShort();
            if (len == 0) return string.Empty;
            var bytes = ReadBytes(len);
            return Encoding.UTF8.GetString(bytes);
        }

        public long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public long Length => stream.Length;

        public void Dispose() => stream.Dispose();
    }
}