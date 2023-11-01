using StunClient.Protocoll.Tools;

namespace StunClient.Protocoll.Attributes
{
    public class Software : MessagePart
    {

        public string Value
        {
            get => value;
            set
            {
                if (value.Length > 128)
                    throw new ArgumentOutOfRangeException("Software attribute can only be 128 characters long");
                this.value = value;
                if (Serialize().Length > 509)
                    throw new ArgumentOutOfRangeException("Software attribute can only be 509 bytes long");
            }
        }
        private string value;

        /// <summary>
        /// Software attribute should include the name and version of the client software.
        /// </summary>
        /// <param name="value"></param>
        /// 
#pragma warning disable CS8618
        public Software(string value)
        {
            Value = value;
        }

        public Software(byte[] data)
        {
            SetupBase(data);
        }
        public Software()
        {
            SetupBase(new byte[0]);
        }
        public Software(Span<byte> data)
        {
            SetupBase(data);
        }
        public Software(Memory<byte> data)
        {
            SetupBase(data.Span);
        }
#pragma warning restore CS8618
        private void SetupBase(Span<byte> data)
        {
            if (data.Length > 763)
                throw new ArgumentOutOfRangeException("Got to much data for software attribute");
            Value = Converter.ToString(data);
        }


        public byte[] Serialize()
        {
            return Converter.ToBytes(Value).ToArray();
        }
    }
}
