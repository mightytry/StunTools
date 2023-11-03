using StunTools;    

namespace TestStun
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run();
        }
        static async Task Run()
        {
            TcpSocket socket = new TcpSocket();
            await socket.UpdateCode();
            Console.WriteLine(socket.Code);
        }
    }
}