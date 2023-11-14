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
            UdpSocket socket = new ();
            await socket.UpdateCode();
            UdpSocket socket2 = new ();
            await socket2.UpdateCode();
            Console.WriteLine(socket2.PublicEndPoint);
            Console.WriteLine(socket.IsReachable);
            Console.WriteLine(socket2.IsReachable);
            socket.SendData("Hello World!", socket2.PublicEndPoint);
            socket2.SendData("Hello World!", socket.PublicEndPoint);
            socket.SendData("Hello World!", socket2.PublicEndPoint);
            Console.WriteLine((await socket2.Receive()).GetData());
            
        }
    }
}