using System.Buffers.Binary;
using System.Text;

namespace StunTools.Protocoll.Tools
{
    internal class Converter
    {
        public static Span<byte> ToBytes(ushort value)
        {
            Span<byte> bytes = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
            return bytes;
        }
        public static Span<byte> ToBytes(int value)
        {
            Span<byte> bytes = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(bytes, value);
            return bytes;
        }

        internal static Span<byte> ToBytes(uint value)
        {
            Span<byte> bytes = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            return bytes;
        }

        internal static Span<byte> ToBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        internal static int ToInt32(Span<byte> data)
        {
            return BinaryPrimitives.ReadInt32BigEndian(data);
        }

        internal static ushort ToUInt16(Span<byte> span)
        {
            return BinaryPrimitives.ReadUInt16BigEndian(span);
        }

        internal static uint ToUInt32(Span<byte> span)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(span);
        }

        internal static string ToString(Span<byte> span)
        {
            return Encoding.UTF8.GetString(span);
        }

    }
}
