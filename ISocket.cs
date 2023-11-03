using StunTools.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StunTools
{
    public abstract class ISocket
    {
        internal abstract Task<IPEndPoint?> GetPublicIp(Protocoll.Message message);
        public IPEndPoint? PublicEndPoint { private set; get; } = null;
        public string? Code { private set; get; } = null;

        public async Task UpdateCode()
        {
            await UpdatePublicEndPoint();
            Code = PublicEndPoint is null ? null : Compressor.Zip(PublicEndPoint);
        }
        public async Task UpdatePublicEndPoint()
        {
            Protocoll.Message message = new Protocoll.Message();
            message.MessageParts.Add(new Protocoll.Attribute(Protocoll.Attribute.AttributeType.SOFTWARE, new Protocoll.Attributes.Software("StunClient - V0.0.1")));
            PublicEndPoint = await GetPublicIp(message);
        }


    }
}
