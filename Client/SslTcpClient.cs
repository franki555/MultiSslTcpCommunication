using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Client
{
    public sealed class SslTcpClient
    {
        private string ipServer;
        private int port;

        private TcpClient client = null;
        public TcpClient Client { get { return client; } }
        private SslStream sslStream = null;

        public SslTcpClient(string __ipServer, int __port)
        {
            ipServer = __ipServer;
            port = __port;
        }

        public void Connect()
        {
            try
            {
                client = new TcpClient(ipServer, port);
            }
            catch
            {
                Console.WriteLine("The server is unavailable");
                return;
            }

            sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(CertificateValidationCallback));

            try
            {
                sslStream.AuthenticateAsClient("clientName");
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine($"Exception: {e.Message}");

                if (e.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                }

                Console.WriteLine("Authentication failed - closing the connection.");

                client.Close();
                return;
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                sslStream.Write(Encoding.UTF8.GetBytes(message + "<EOF>"));
                sslStream.Flush();
            }
            catch
            {
                Console.WriteLine("The server is unavailable");

                if (client != null)
                {
                    client.Close();
                }
                return;
            }
        }

        public string ReadMessage()
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            StringBuilder messageData = new StringBuilder();

            int bytesRead = -1;
            do
            {
                bytesRead = sslStream.Read(buffer, 0, client.ReceiveBufferSize);

                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytesRead)];
                decoder.GetChars(buffer, 0, bytesRead, chars, 0);
                messageData.Append(chars);

                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytesRead != 0);

            return messageData.ToString().Substring(0, messageData.Length - 5);
        }

        public void Disconnect()
        {
            client.Close();
        }

        static bool CertificateValidationCallback(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
