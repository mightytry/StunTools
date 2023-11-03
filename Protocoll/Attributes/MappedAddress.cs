using StunTools.Protocoll.Tools;
using System.Net;

namespace StunTools.Protocoll.Attributes
{
    public class MappedAddress : MessagePart
    {
        public ushort Family { get; private set; }
        public ushort Port { get; private set; }
        public uint Address { get; private set; }
        public IPEndPoint EndPoint { get { return new IPEndPoint(Address, Port); } }

        public MappedAddress(byte[] data)
        {
            SetupMappedAddress(data);
        }
        public MappedAddress(Span<byte> data)
        {
            SetupMappedAddress(data);
        }
        public MappedAddress(Memory<byte> data)
        {
            SetupMappedAddress(data.Span);
        }

        public void SetupMappedAddress(Span<byte> data)
        {
            Family = Converter.ToUInt16(data[..(int)MappedAddressValues.Family_Bytes]);
            Port = Converter.ToUInt16(data[(int)MappedAddressValues.Family_Bytes..((int)MappedAddressValues.Family_Bytes + (int)MappedAddressValues.Port_Bytes)]);
            Address = Converter.ToUInt32(data[((int)MappedAddressValues.Family_Bytes + (int)MappedAddressValues.Port_Bytes)..]);
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
            data.Write(Converter.ToBytes(Port));
            data.Write(Converter.ToBytes(Address));
            return data.ToArray();
        }
    }
}
