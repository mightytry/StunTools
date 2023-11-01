using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StunClient
{
    public class TcpClient
    {
        private Socket socket;

        public TcpClient(Socket s)
        {
            socket = s;
        }


        public async Task<Message?> Receive()
        {
            Packet result = new Packet();
            while (true)
            {
                try
                {
                    ArraySegment<byte> buffer = new(result.Buffer, result.Offset, result.Size);
                    var read = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (result.Receive(read))
                    {
                        return Message.Deserilize(Encoding.UTF8.GetString(result.Buffer), socket.RemoteEndPoint as IPEndPoint);
                    }
                }
                catch
                {
                    continue;
                }

            }
        }

        private async Task sendData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            await socket.SendAsync(BitConverter.GetBytes(bytes.Length), SocketFlags.None);
            for (int i = 0; i < bytes.Length; i += Packet.BUFFER_SIZE)
            {
                await socket.SendAsync(bytes[i..Math.Min(Packet.BUFFER_SIZE + i, bytes.Length)], SocketFlags.None);
                await Task.Delay(1); // To prevent UDP packet loss limits troughput to 1MB/s
            }
        }

        public async Task SendData(object obj)
        {
            await sendData(new Message(obj).Serialize());
        }

        public async Task SendData(Message message)
        {
            await sendData(message.Serialize());
        }

        public void Close()
        {
            socket.Close();
        }
    }
}
