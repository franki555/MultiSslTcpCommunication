using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Server
{
    public sealed class ClientObject
    {
        private TcpClient client = null;
        private IPAddress ipClient = null;

        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;

            ipClient = ((IPEndPoint)client.Client.LocalEndPoint).Address;
            Console.WriteLine($"[{ipClient.ToString()}] Client connected");
        }

        public void Process()
        {
            SslStream sslStream = new SslStream(client.GetStream(), false);

            try
            {
                var certificate = new X509Certificate2("server.pfx", "password");

                sslStream.AuthenticateAsServer(certificate,
                                                clientCertificateRequired: false,
                                                SslProtocols.None,
                                                checkCertificateRevocation: false);

                sslStream.ReadTimeout = 5000;
                sslStream.WriteTimeout = 5000;


                Console.WriteLine($"[{ipClient.ToString()}] Waiting for client message...");

                string request = "";
                try
                {
                    request = ReadMessage(sslStream);
                }
                catch
                {
                    Console.WriteLine($"[{ipClient.ToString()}] Waiting time exceeded");

                    sslStream.Close();
                    client.Close();
                    return;
                }

                Console.WriteLine($"[{ipClient.ToString()}] Received: {request}");

                string response = request;
                sslStream.Write(Encoding.UTF8.GetBytes(response + "<EOF>"));

                Console.WriteLine($"[{ipClient.ToString()}] Sending: {response}");

                Console.WriteLine($"[{ipClient.ToString()}] Client disconnected");
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine($"[{ipClient.ToString()}] Exception: {e.Message}");

                if (e.InnerException != null)
                {
                    Console.WriteLine($"[{ipClient.ToString()}] Inner exception: {e.InnerException.Message}");
                }

                Console.WriteLine($"[{ipClient.ToString()}] Authentication failed - closing the connection.");

                sslStream.Close();
                client.Close();
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                sslStream.Close();
                client.Close();
                return;
            }
            finally
            {
                sslStream.Close();
                client.Close();
            }
        }

        private string ReadMessage(SslStream sslStream)
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
    }
}
