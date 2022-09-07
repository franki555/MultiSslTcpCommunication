using System.Net;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 5555;
            SslTcpServer.RunServer(IPAddress.Any, Port);
        }
    }
}
