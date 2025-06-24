using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using arriety.utils;
using arriety.utils.network;

namespace arriety.login.network
{
    public class NetworkManager : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;

        private Thread receiveThread;
        private Thread sendThread;
        private readonly object disconnectLock = new();

        private volatile bool running;
        private byte[] buffer = new byte[8192];
        private int bufferLen;

        public int SessionId { get; set; }

        private ConcurrentQueue<byte[]> sendQueue = new();

        public event Action OnDisconnected;

        public void Connect(string host, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(host, port);
                stream = client.GetStream();

                running = true;

                receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
                sendThread = new Thread(SendLoop) { IsBackground = true };
                receiveThread.Start();
                sendThread.Start();

                Log.Info($"[Network Login] Connected to {host}:{port}");
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void SendPacket(ClientPacket packet)
        {
            if (!running || stream == null)
            {
                Log.Warning("[Network] Attempted to send packet while disconnected.");
                return;
            }

            byte[] payload;
            using (var pw = new PacketWriter())
            {
                pw.WriteByte(packet.OpCode);
                packet.Write(pw);
                payload = pw.ToArray();
            }

            SendPacket(payload);
        }

        public void SendPacket(byte[] data)
        {
            if (!running || stream == null) return;

            using (var pw = new PacketWriter())
            {
                pw.WriteUShort((ushort)(data.Length + 2));
                pw.WriteBytes(data);
                sendQueue.Enqueue(pw.ToArray());
            }
        }

        private void ReceiveLoop()
        {
            var tmp = new byte[4096];
            try
            {
                while (running)
                {
                    var count = stream.Read(tmp, 0, tmp.Length);
                    if (count <= 0)
                    {
                        Disconnect();
                        break;
                    }

                    if (bufferLen + count > buffer.Length)
                        ExpandBuffer(bufferLen + count);

                    Array.Copy(tmp, 0, buffer, bufferLen, count);
                    bufferLen += count;

                    ProcessReadBuffer();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                Disconnect();
            }
        }

        private void ProcessReadBuffer()
        {
            var offset = 0;
            while (bufferLen - offset >= 3)
            {
                var length = (ushort)(buffer[offset] | (buffer[offset + 1] << 8));

                if (length < 3)
                {
                    Log.Warning($"[Network] Invalid packet length: {length}");
                    Disconnect();
                    return;
                }

                if (bufferLen - offset < length)
                    break;

                var opcode = buffer[offset + 2];
                var payloadLen = length - 3;
                var payload = new byte[payloadLen];
                Array.Copy(buffer, offset + 3, payload, 0, payloadLen);

                Log.Info($"[Network] Received opcode 0x{opcode:X2}, length: {length}");

                HandlePacket(opcode, payload);

                offset += length;
            }

            if (offset > 0)
            {
                Array.Copy(buffer, offset, buffer, 0, bufferLen - offset);
                bufferLen -= offset;
            }
        }

        private void HandlePacket(byte opcode, byte[] payload)
        {
            var packet = ServerPacketFactory.Create(opcode);
            if (packet == null)
            {
                Log.Warning($"[Network] Unknown opcode: 0x{opcode:X2}");
                return;
            }

            using var reader = new PacketReader(payload);
            packet.Read(reader);
            packet.Run();
        }

        private void ExpandBuffer(int minSize)
        {
            var newSize = buffer.Length * 2;
            while (newSize < minSize) newSize *= 2;

            var newBuffer = new byte[newSize];
            Array.Copy(buffer, 0, newBuffer, 0, bufferLen);
            buffer = newBuffer;
        }

        private void SendLoop()
        {
            try
            {
                while (running)
                {
                    if (sendQueue.TryDequeue(out var data))
                    {
                        stream.Write(data, 0, data.Length);
                        Log.Info($"[Network] Sent packet: {BitConverter.ToString(data)}");
                    }
                    else
                    {
                        Thread.Sleep(2);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
                Disconnect();
            }
        }

        private void Disconnect()
        {
            lock (disconnectLock)
            {
                if (!running) return;
                running = false;

                try { stream?.Close(); } catch { }
                try { client?.Close(); } catch { }

                Log.Info("[Network] Disconnected.");

                OnDisconnected?.Invoke();
            }
        }

        public void Dispose() => Disconnect();
    }
}
