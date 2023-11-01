
using StunClient.Tools;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace StunClient
{
    public class TcpSocket
    {
        public IPEndPoint? LocalEndPoint { get => Socket?.LocalEndPoint as IPEndPoint; }
        public readonly Socket? Socket;
        //public List<TcpClient> Clients { get; internal set; } = new List<TcpClient>();

        public TcpSocket()
        {
            Socket = CreateSocket();
        }

        private Socket CreateSocket()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            s.Bind(Socket?.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0));
            return s;
        }

        public async Task<TcpClient?> Connenct(IPEndPoint endPoint)
        {
            Socket s = CreateSocket();
            await s.ConnectAsync(endPoint.Address, endPoint.Port);
            return new TcpClient(s);
        }

        public async Task<IPEndPoint?> GetPublicEndPoint()
        {
            Protocoll.Message message = new();
            message.MessageParts.Add(new Protocoll.Attribute(Protocoll.Attribute.AttributeType.SOFTWARE, new Protocoll.Attributes.Software("StunClient - V0.0.1")));

            return await GetPublicIpTcp(message);
        }

        public async Task<string?> GetCode()
        {
            if (await GetPublicEndPoint() is IPEndPoint endPoint)
            {
                return Compressor.Zip(endPoint);
            }
            return null;
        }


        private async Task<IPEndPoint?> GetPublicIpTcp(Protocoll.Message message)
        {
            byte[] bytes = message.Serialize();
            CancellationTokenSource cts = new CancellationTokenSource(Hosts.TIMEOUT);
            IPEndPoint? ress = null;
            _ = await Task.WhenAny(Hosts.List.Select(async (x) =>
            {
                try
                {
                    if (cts.IsCancellationRequested)
                        return;
                    using Socket s = CreateSocket();
                    await s.ConnectAsync((await Hosts.SolveDns(x.Key, x.Value))!, cts.Token);
                    await s.SendAsync(bytes, SocketFlags.None, cts.Token);
                    byte[] res = new byte[1024];
                    await s.ReceiveAsync(res, SocketFlags.None, cts.Token);
                    ress = (new Protocoll.Message(res).TryGetAnyEndPoint())!;
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
