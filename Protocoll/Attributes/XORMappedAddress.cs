using StunClient.Protocoll.Tools;
using System.Net;

namespace StunClient.Protocoll.Attributes
{
    public class XORMappedAddress : MessagePart
    {
        public ushort Family { get; set; }
        public ushort Port { get; set; }
        public uint Address { get; set; }
        public IPEndPoint EndPoint { get { return new IPEndPoint(Address, Port); } }

        public XORMappedAddress(byte[] data)
        {
            SetupMappedAddress(data);
        }
        public XORMappedAddress(Span<byte> data)
        {
            SetupMappedAddress(data);
        }
        public XORMappedAddress(Memory<byte> data)
        {
            SetupMappedAddress(data.Span);
        }

        public void SetupMappedAddress(Span<byte> data)
        {
            Family = Converter.ToUInt16(data[..(int)MappedAddressValues.Family_Bytes]);
            Port = Converter.ToUInt16(MagicCookie.DecodeXOR(data[(int)MappedAddressValues.Family_Bytes..((int)MappedAddressValues.Family_Bytes + (int)MappedAddressValues.Port_Bytes)]));
            Address = Converter.ToUInt32(MagicCookie.DecodeXOR(data[((int)MappedAddressValues.Family_Bytes + (int)MappedAddressValues.Port_Bytes)..]));
        }

        public enum MappedAddressValues : ushort
        {
            Family_Bytes = 2,
            Port_Bytes = 2,
            Address_Bytes = 4,
        }

        public byte[] Serialize()
        {
            MemoryStream data = new MemoryStream();
            data.Write(Converter.ToBytes(Family));
            data.Write(MagicCookie.DecodeXOR(Converter.ToBytes(Port)));
            data.Write(MagicCookie.DecodeXOR(Converter.ToBytes(Address)));
            return data.ToArray();
        }
    }
}
