using System.Net;

namespace StunTools.Protocoll
{
    public class Message
    {
        public Header Header { get; private set; }
        public List<Attribute> MessageParts { get; private set; }

        public Message()
        {
            Header = new Header(20);
            MessageParts = new List<Attribute>();
        }

#pragma warning disable CS8618
        public Message(byte[] data)
        {
            SetupMessage(data);
        }

        public Message(Span<byte> data)
        {
            SetupMessage(data);
        }

        public Message(Memory<byte> data)
        {
            SetupMessage(data.Span);
        }
#pragma warning restore CS8618 

        public void SetupMessage(Span<byte> data)
        {
            Header = new Header(data[MessageValues.HeaderRange]);
            MessageParts = new List<Attribute>();
            MemoryStream ms = new MemoryStream(data[MessageValues.HeaderRange.End..].ToArray());
            while (ms.Position < ms.Length)
            {
                MessageParts.Add(Attribute.FromStream(ref ms));
            }
        }

        public T? TryGetAttribute<T>() where T : MessagePart
        {
            return (T?)MessageParts.FirstOrDefault(x => x.Value is T)?.Value;
        }

        public IPEndPoint? TryGetAnyEndPoint()
        {
            return TryGetAttribute<Attributes.MappedAddress>()?.EndPoint ?? TryGetAttribute<Attributes.XORMappedAddress>()?.EndPoint;
        }

        private sealed class MessageValues
        {
            public readonly static Range HeaderRange = 0..20;
        }

        public byte[] Serialize()
        {
            MemoryStream ms = new MemoryStream();
            foreach (var part in MessageParts)
            {
                ms.Write(part.Serialize());
            }
            Header.MessageLength = (ushort)ms.Length;
            return Header.Serialize().Concat(ms.ToArray()).ToArray();
        }

        public bool Validate()
        {
            return false;
        }
    }
}
