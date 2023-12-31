﻿
using StunTools.Tools;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace StunTools
{
    public class UdpSocket: ISocket
    {
        public readonly Socket Socket;
        protected override IPEndPoint? localEndPoint { get => Socket.LocalEndPoint as IPEndPoint; }

        private HashSet<IPEndPoint> stunEndPoints = new HashSet<IPEndPoint>();

        public UdpSocket()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }


        public async Task<Message> Receive()
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
                            Message? message = Message.Deserilize(Encoding.UTF8.GetString(result.Buffer), read.RemoteEndPoint as IPEndPoint);
                            if (message is not null)
                            {
                                return message;
                            }
                            throw new Exception();
                        }
                    }
                }
                catch 
                {
                    result = new Packet();
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


        internal override async Task<(AdressBehaviorType, IPEndPoint?)> GetPublicIp(Protocoll.Message message)
        {
            byte[] bytes = message.Serialize();
            CancellationTokenSource cts = new CancellationTokenSource(Hosts.TIMEOUT);
            IPEndPoint? ress = null;
            IPEndPoint? msg = null;
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
            return ((ress?.Equals(msg) ?? true) ? AdressBehaviorType.EndPointIndependent : AdressBehaviorType.EndPointDependent, ress);
        }

        protected override async Task<bool> isReachable()
        {
            UdpSocket s = new UdpSocket();
            await s.UpdateCode(false);
            await SendData("Test", s.PublicEndPoint);
            await s.SendData("Test", PublicEndPoint!);
            try
            {
                return (await Receive().WaitAsync(TimeSpan.FromMilliseconds(500))).GetData() == "Test";
            }
            catch
            {
                return false;
            }
        }
    }
}
