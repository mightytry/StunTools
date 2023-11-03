namespace StunTools.Protocoll.Attributes
{
    public class Base : MessagePart
    {
        public byte[] Value { get; set; }
#pragma warning disable CS8618
        public Base(byte[] data)
        {
            SetupBase(data);
        }
        public Base()
        {
            SetupBase(new byte[0]);
        }
        public Base(Span<byte> data)
        {
            SetupBase(data);
        }
        public Base(Memory<byte> data)
        {
            SetupBase(data.Span);
        }
#pragma warning restore CS8618
        private void SetupBase(Span<byte> data)
        {
            Value = data.ToArray();
        }

        public byte[] Serialize()
        {
            return Value;
        }
    }
}
