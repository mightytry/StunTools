
using StunTools.Tools;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace StunTools
{
    public class TcpSocket : ISocket , IDisposable
    {
        public readonly Socket? Socket;
        protected override IPEndPoint? localEndPoint { get => Socket?.LocalEndPoint as IPEndPoint; }

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
            while (true)
            {
                try
                {
                    await s.ConnectAsync(endPoint.Address, endPoint.Port);
                }
                catch (SocketException)
                {
                    continue;
                }
                break;
            }
            return new TcpClient(s);
        }

        internal override async Task<(AdressBehaviorType, IPEndPoint?)> GetPublicIp(Protocoll.Message message)
        {
            byte[] bytes = message.Serialize();
            CancellationTokenSource cts = new CancellationTokenSource(Hosts.TIMEOUT);
            IPEndPoint? ress = null;
            IPEndPoint? msg = null;
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
                    msg = new Protocoll.Message(res).TryGetAnyEndPoint();
                    if (msg is null)
                        throw new Exception();
                    else if (ress is null)
                    {
                        ress = msg;
                        throw new Exception();
                    }
                    cts.Cancel();
                    return;
                }
                catch
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
            
            return ((ress?.Equals(msg)?? true)? AdressBehaviorType.EndPointIndependent: AdressBehaviorType.EndPointDependent, ress);
        }

        protected override async Task<bool> isReachable()
        {
            using TcpSocket s = new TcpSocket();
            await s.UpdateCode(false);
            _ = s.Connenct(PublicEndPoint);
            try
            {
                await Connenct(s.PublicEndPoint!).WaitAsync(TimeSpan.FromMilliseconds(500));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Socket.Dispose();
        }
    }
}
