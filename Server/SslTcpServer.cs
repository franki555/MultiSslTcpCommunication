using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    public sealed class SslTcpServer
    {
        public static void RunServer(IPAddress ipServer, int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(ipServer, port);
                listener.Start();
                while (true)
                {
                    Console.WriteLine("Waiting for a client to connect...");

                    var client = listener.AcceptTcpClient();
                    (new Thread(new ThreadStart((new ClientObject(client)).Process))).Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }
    }
}
