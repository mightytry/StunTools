using StunTools.Protocoll.Tools;

namespace StunTools.Protocoll
{
    public class Header : MessagePart
    {
        public ushort MessageLength { get; set; }
        public MessageClassTypes MessageClass { get; set; }
        public MethodTypes Method { get; set; }
        public MagicCookie MagicCookie { get; set; }
        public byte[] TransactionId { get; set; }

#pragma warning disable CS8618
        public Header(byte[] data)
        {
            SetupHeader(data);
        }

        public Header(Memory<byte> data)
        {
            SetupHeader(data.Span);
        }

        public Header(Span<byte> data)
        {
            SetupHeader(data);
        }
#pragma warning restore CS8618

        private void SetupHeader(Span<byte> data)
        {
            encodeType(Converter.ToUInt16(data[HeaderValues.Type_Bytes]));
            MessageLength = Converter.ToUInt16(data[HeaderValues.Length_Bytes]);
            MagicCookie = new MagicCookie(data[HeaderValues.MagicCookie_Bytes]);
            TransactionId = data[HeaderValues.TransactionId_Bytes].ToArray();
        }
        // https://datatracker.ietf.org/doc/html/rfc8489#appendix-A
        private void encodeType(ushort type)
        {
            MessageClass = (MessageClassTypes)((type & 0x0100) >> 7 | (type & 0x0010) >> 4);
            Method = (MethodTypes)((type & 0x3E00) >> 2 | (type & 0x00E0) >> 1 | type & 0x000F);
        }

        private ushort decodeType()
        {
            int method = (int)Method;
            int cls = (int)MessageClass;
            return (ushort)((method & 0x1F80) << 2 | (method & 0x0070) << 1 | method & 0x000F | (cls & 0x0002) << 7 | (cls & 0x0001) << 4);
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(Converter.ToBytes(decodeType()));
            stream.Write(Converter.ToBytes(MessageLength));
            stream.Write(MagicCookie.Serialize());
            stream.Write(TransactionId);
            return stream.ToArray();
        }

        public Header(ushort messageLength, MessageClassTypes messageClass = MessageClassTypes.Request, MethodTypes method = MethodTypes.Binding, MagicCookie? magicCookie = null, byte[]? transactionId = null)
        {
            MessageLength = messageLength;
            MessageClass = messageClass;
            Method = method;
            MagicCookie = magicCookie ?? new MagicCookie();
            TransactionId = CheckTransactionId(transactionId);
        }

        private static byte[] CheckTransactionId(byte[]? transactionId)
        {
            if (transactionId == null)
            {
                Random r = new Random();
                byte[] data = new byte[12];
                r.NextBytes(data);
                return data;
            }
            if (transactionId.Length != 12)
            {
                throw new ArgumentException("TransactionId must be 12 bytes long");
            }
            return transactionId;
        }

        private sealed class HeaderValues
        {
            // Length of the attribute in bytes
            public static readonly Range Type_Bytes = 0..2;
            public static readonly Range Length_Bytes = 2..4;
            public static readonly Range MagicCookie_Bytes = 4..8;
            public static readonly Range TransactionId_Bytes = 8..20;
        }

        public enum MessageClassTypes : ushort
        {
            Request = 0b00,
            Indication = 0b01,
            Success = 0b10,
            Error = 0b11,
        }

        public enum MethodTypes : ushort
        {
            Binding = 0x0001,
        }
    }
}
