using System.Net;
using System.Net.NetworkInformation;

namespace StunTools.Tools
{
    public static class Compressor
    {
        internal const string legal_chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!?-";

        public static string Zip(IPEndPoint endPoint)
        {
            string ip = endPoint.Address.ToString();

            int port = endPoint.Port;

            return Zip(ip, port);
        }


        public static string Zip(string ip, int port)
        {
            ulong i = BitConverter.ToUInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);

            i <<= 16;
            i |= (uint)port;

            string code = "";
            while (i > 0)
            {
                code = legal_chars[(int)(i % (ulong)legal_chars.Length)] + code;

                i = i / (ulong)legal_chars.Length;
            }

            return code;
        }

        public static IPEndPoint UnZip(string code)
        {
            ulong i = 0;
            for (int c = 0; c < code.Length; c++)
            {
                i = i * (ulong)legal_chars.Length + (ulong)legal_chars.IndexOf(code[c]);
            }
            IPAddress ip = new IPAddress((uint)(i >> 16));

            return new IPEndPoint(ip, (int)(i & 0xffff));
        }
    }
}
