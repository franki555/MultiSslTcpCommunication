using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            SslTcpClient sslTcpClient = new SslTcpClient("127.0.0.1", 5555);
            sslTcpClient.Connect();
            sslTcpClient.SendMessage("Hello World!");
            string serverMessage = sslTcpClient.ReadMessage();
            sslTcpClient.Disconnect();

            Console.WriteLine(serverMessage);
        }
    }
}
