using System.Net.Sockets;

namespace StunClient
{
    public class Packet
    {
        public const int BUFFER_SIZE = 1024;

        public byte[] Buffer;

        public int Length;
        private int readLength;

        public int Offset { get => readLength; }
        public int Size { get => Math.Min(BUFFER_SIZE, Buffer.Length - readLength); }

        public SocketFlags SocketFlag = SocketFlags.None;

        public Packet()
        {
            Buffer = new byte[4]; // Int size = 4
            readLength = 0;
            Length = -1;
        }

        public bool Receive(int read)
        {
            if (Length == -1)
            {
                Length = BitConverter.ToInt32(Buffer, 0);
                Buffer = new byte[Length];
                readLength = 0;
            }
            else
            {
                readLength += read;
                if (readLength == Length)
                {
                    Length = -1;
                    readLength = 0;
                    return true;
                }
            }
            return false;
        }
    }
}
