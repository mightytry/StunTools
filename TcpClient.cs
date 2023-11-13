using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StunTools
{
    public class TcpClient
    {
        public Socket Socket;
        public bool Connected { get => Socket.Connected; }

        public TcpClient(Socket s)
        {
            Socket = s;
        }


        public async Task<Message> Receive()
        {
            Packet result = new Packet();
            while (true)
            {
                try
                {
                    ArraySegment<byte> buffer = new(result.Buffer, result.Offset, result.Size);
                    var read = await Socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (result.Receive(read))
                    {
                        Message? message = Message.Deserilize(Encoding.UTF8.GetString(result.Buffer), Socket.RemoteEndPoint as IPEndPoint);
                        if (message is not null)
                        {
                            return message;
                        }
                        throw new Exception();
                    }
                }
                catch
                {
                    result = new Packet();
                    continue;
                }

            }
        }

        private async Task sendData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            await Socket.SendAsync(BitConverter.GetBytes(bytes.Length), SocketFlags.None);
            for (int i = 0; i < bytes.Length; i += Packet.BUFFER_SIZE)
            {
                await Socket.SendAsync(bytes[i..Math.Min(Packet.BUFFER_SIZE + i, bytes.Length)], SocketFlags.None);
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
            Socket.Close();
        }
    }
}
