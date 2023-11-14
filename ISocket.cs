using StunTools.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StunTools
{
    public abstract class ISocket
    {
        internal abstract Task<(AdressBehaviorType, IPEndPoint?)> GetPublicIp(Protocoll.Message message);

        public IPEndPoint? PublicEndPoint { private set; get; } = null;
        public string? Code { private set; get; } = null;

        public AdressBehaviorType MappingBehavior { private set; get; }
        public bool IsReachable { private set; get;}

        public IPEndPoint? LocalEndPoint { get => localEndPoint.Address.Equals(IPAddress.Any) ? new IPEndPoint(getLocalIp() ?? IPAddress.Any, localEndPoint.Port) : localEndPoint; }
        protected abstract IPEndPoint localEndPoint { get; }

        public async Task UpdateCode(bool checkReachable = true)
        {
            await UpdatePublicEndPoint(checkReachable);
            Code = Compressor.Zip((PublicEndPoint is null || !IsReachable) ? LocalEndPoint : PublicEndPoint);
        }
        public async Task UpdatePublicEndPoint(bool checkReachable = true)
        {
            Protocoll.Message message = new Protocoll.Message();
            message.MessageParts.Add(new Protocoll.Attribute(Protocoll.Attribute.AttributeType.SOFTWARE, new Protocoll.Attributes.Software("StunClient - V0.0.1")));
            var x = await GetPublicIp(message);
            MappingBehavior = x.Item1;
            PublicEndPoint = x.Item2;
            if (checkReachable)
                IsReachable = await isReachable();
        }

        private static IPAddress? getLocalIp()
        {
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault((n) =>
                       n != null &&
                       n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                       n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                       n.IsReceiveOnly == false &&
                      !n.Name.StartsWith("vEthernet") &&
                       n.OperationalStatus == OperationalStatus.Up
            , null)?.GetIPProperties().UnicastAddresses.First((a) => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Address;
        }

        protected abstract Task<bool> isReachable();

    }
    public enum AdressBehaviorType
    {
        EndPointIndependent,
        EndPointDependent,
    }
}
