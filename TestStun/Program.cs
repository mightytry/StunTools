using Microsoft.VisualBasic;
using StunTools;
using StunTools.Tools;

namespace TestStun
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }
        static async Task Run()
        {
            TcpSocket socket = new TcpSocket();
            await socket.UpdateCode();
            Console.WriteLine(Compressor.Zip(socket.LocalEndPoint));
            Console.WriteLine(Compressor.UnZip("3vCU6JlB"));
        }
    }
}