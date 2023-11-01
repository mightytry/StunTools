
using StunClient.Tools;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace StunClient
{
    public class UdpSocket
    {
        public readonly Socket Socket;
        public IPEndPoint? LocalEndPoint { get => Socket.LocalEndPoint as IPEndPoint; }

        private HashSet<IPEndPoint> stunEndPoints = new HashSet<IPEndPoint>();

        public UdpSocket()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        public async Task<IPEndPoint?> GetPublicEndPoint()
        {
            Protocoll.Message message = new Protocoll.Message();
            message.MessageParts.Add(new Protocoll.Attribute(Protocoll.Attribute.AttributeType.SOFTWARE, new Protocoll.Attributes.Software("StunClient - V0.0.1")));

            return await GetPublicIpUdp(message);
        }

        public async Task<string?> GetCode()
        {
            if (await GetPublicEndPoint() is IPEndPoint endPoint)
            {
                return Compressor.Zip(endPoint);
            }
            return null;
        }

        public async Task<Message?> Receive()
        {
            Packet result = new Packet();
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try {
                    ArraySegment<byte> buffer = new(result.Buffer, result.Offset, result.Size);
                    var read = await Socket.ReceiveFromAsync(buffer, SocketFlags.None, remote);
                    if (!stunEndPoints.Contains((read.RemoteEndPoint as IPEndPoint)!))
                    {
                        if (result.Receive(read.ReceivedBytes))
                        {
                            return Message.Deserilize(Encoding.UTF8.GetString(result.Buffer), read.RemoteEndPoint as IPEndPoint);
                        }
                    }
                }
                catch
                {
                    continue;
                }

            }
        }

        private async Task sendData(string data, IPEndPoint endPoint)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            await Socket.SendToAsync(BitConverter.GetBytes(bytes.Length), SocketFlags.None, endPoint);
            for (int i = 0; i < bytes.Length; i += Packet.BUFFER_SIZE)
            {
                await Socket.SendToAsync(bytes[i..Math.Min(Packet.BUFFER_SIZE + i, bytes.Length)], SocketFlags.None, endPoint);
                await Task.Delay(1); // To prevent UDP packet loss limits troughput to 1MB/s
            }
        }

        public async Task SendData(object obj, IPEndPoint endPoint)
        {
            await sendData(new Message(obj).Serialize(), endPoint);
        }

        public async Task SendData(Message message, IPEndPoint endPoint)
        {
            await sendData(message.Serialize(), endPoint);
        }


        private async Task<IPEndPoint?> GetPublicIpUdp(Protocoll.Message message)
        {
            byte[] bytes = message.Serialize();
            CancellationTokenSource cts = new CancellationTokenSource(Hosts.TIMEOUT);
            IPEndPoint? ress = null;
            await Task.WhenAny(Hosts.List.Select(async (x) =>
            {
                try
                {
                    if (cts.IsCancellationRequested)
                        return;
                    IPEndPoint ep = (await Hosts.SolveDns(x.Key, x.Value))!;
                    stunEndPoints.Add(ep);
                    await Socket.SendToAsync(bytes, SocketFlags.None, ep, cts.Token);
                    byte[] res = new byte[1024];
                    await Socket.ReceiveAsync(res, SocketFlags.None, cts.Token);
                    ress = new Protocoll.Message(res).TryGetAnyEndPoint();
                    cts.Cancel();
                    return;
                }
                catch (Exception)
                {
                    try
                    {
                        await Task.Delay(Hosts.TIMEOUT, cts.Token);
                    }
                    catch
                    {
                        
                    }
                    return;
                }
                
            }).ToArray());
            return ress;
        }
    }
}
