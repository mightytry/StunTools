using StunTools.Protocoll.Tools;

namespace StunTools.Protocoll
{
    public class MagicCookie : MessagePart
    {
        public const int MagicCookieFixedValue = 0x2112A442;

        public int MagicCookieValue { get; private set; }

        public MagicCookie()
        {
            MagicCookieValue = MagicCookieFixedValue;
        }

        public MagicCookie(byte[] data)
        {
            SetupMagicCookie(data);
        }

        public MagicCookie(Memory<byte> data)
        {
            SetupMagicCookie(data.Span);
        }

        public MagicCookie(Span<byte> data)
        {
            SetupMagicCookie(data);
        }

        private void SetupMagicCookie(Span<byte> data)
        {
            MagicCookieValue = Converter.ToInt32(data);
            if (!Validate())
            {
                throw new Exception("Got invalid magic cookie");
            }
        }

        public static Span<byte> DecodeXOR(Span<byte> data)
        {
            Span<byte> mg = Converter.ToBytes(MagicCookieFixedValue);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= mg[i];
            }
            return data;
        }

        public byte[] Serialize()
        {
            return Converter.ToBytes(MagicCookieValue).ToArray();
        }

        public bool Validate()
        {
            return MagicCookieValue == MagicCookieFixedValue;
        }
    }
}
