using StunClient.Protocoll.Tools;

namespace StunClient.Protocoll
{
    public class Attribute : MessagePart
    {
        public AttributeType Type { get; private set; }
        public ushort Length { get; private set; }
        public MessagePart Value { get; private set; }

        public Attribute(AttributeType attributeType, MessagePart messagePart)
        {
            Type = attributeType;
            Length = 0;
            Value = messagePart;
        }
#pragma warning disable CS8618
        public Attribute(byte[] data)
        {
            SetupAttribute(data);
        }

        public Attribute(Memory<byte> data)
        {
            SetupAttribute(data.Span);
        }

        public Attribute(Span<byte> data)
        {
            SetupAttribute(data);
        }
#pragma warning restore CS8618

        public static Attribute FromStream(ref MemoryStream stream)
        {
            Span<byte> data = new byte[4];
            stream.Read(data);
            //var type = (AttributeType)Converter.ToUInt16(data[..(int)AttributeValues.Type_Bytes]);
            var length = Converter.ToUInt16(data[(int)AttributeValues.Type_Bytes..((int)AttributeValues.Type_Bytes + (int)AttributeValues.Length_Bytes)]);
            Span<byte> data2 = new byte[length + 4];
            data.CopyTo(data2);
            stream.Read(data2[4..]);
            return new Attribute(data2);
        }

        private void SetupAttribute(Span<byte> data)
        {
            Type = (AttributeType)Converter.ToUInt16(data[..(int)AttributeValues.Type_Bytes]);
            Length = Converter.ToUInt16(data[(int)AttributeValues.Type_Bytes..((int)AttributeValues.Type_Bytes + (int)AttributeValues.Length_Bytes)]);
            Value = getValue(data[(int)AttributeValues.Value_Start..]);
        }

        private MessagePart getValue(Span<byte> data)
        {
            return Type switch
            {
                AttributeType.MAPPED_ADDRESS => new Attributes.MappedAddress(data),
                AttributeType.XOR_MAPPED_ADDRESS => new Attributes.XORMappedAddress(data),
                AttributeType.SOFTWARE => new Attributes.Software(data),
                _ => new Attributes.Base(data),
            };
        }

        public byte[] Serialize()
        {
            MemoryStream stream = new();
            byte[] val = Value.Serialize();
            Length = (ushort)val.Length;
            stream.Write(Converter.ToBytes((ushort)Type));
            stream.Write(Converter.ToBytes(Length));
            stream.Write(getPaddedValue(val));
            return stream.ToArray();
        }

        private byte[] getPaddedValue(byte[] data)
        {
            byte[] bytes = new byte[Length + (4 - Length % 4) % 4];
            data.CopyTo(bytes, 0);
            return bytes;
        }

        // 0                   1                   2                   3
        // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //|         Type                  |            Length             |
        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        //|                         Value(variable)                ....
        //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

        public enum AttributeValues : ushort
        {
            // Length of the attribute in bytes
            Type_Bytes = 2,
            Length_Bytes = 2,
            Value_Start = 4,
        }

        public enum AttributeType : ushort
        {
            //Comprehension-required range (0x0000-0x7FFF):
            Reserved = 0x0000,
            MAPPED_ADDRESS = 0x0001,
            RESPONSE_ADDRESS = 0x0002, // Reserved; was RESPONSE-ADDRESS prior to[RFC5389]
            CHANGE_REQUEST = 0x0003, // Reserved; was CHANGE-REQUEST prior to[RFC5389]
            SOURCE_ADDRESS = 0x0004, // Reserved; was SOURCE-ADDRESS prior to[RFC5389]
            CHANGED_ADDRESS = 0x0005, // Reserved; was CHANGED-ADDRESS prior to[RFC5389]
            USERNAME = 0x0006,
            PASSWORD = 0x0007, // Reserved; was PASSWORD prior to[RFC5389]
            MESSAGE_INTEGRITY = 0x0008,
            ERROR_CODE = 0x0009,
            UNKNOWN_ATTRIBUTES = 0x000A,
            REFLECTED_FROM = 0x000B, // Reserved; was REFLECTED-FROM prior to[RFC5389]
            REALM = 0x0014,
            NONCE = 0x0015,
            XOR_MAPPED_ADDRESS = 0x0020,

            //Comprehension-optional range(0x8000-0xFFFF)
            SOFTWARE = 0x8022,
            ALTERNATE_SERVER = 0x8023,
            FINGERPRINT = 0x8028,

            // New attributes
            // https://datatracker.ietf.org/doc/html/rfc8489#section-18.3.2
            //   Comprehension-required range (0x0000-0x7FFF):
            MESSAGE_INTEGRITY_SHA256 = 0x001C,
            PASSWORD_ALGORITHM = 0x001D,
            USERHASH = 0x001E,

            //Comprehension-optional range (0x8000-0xFFFF)
            PASSWORD_ALGORITHMS = 0x8002,
            ALTERNATE_DOMAIN = 0x8003,

        }
    }
}
