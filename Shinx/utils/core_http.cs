using Cosmos.System.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Shinx.Commands
{
    internal class core_http : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("server: no network connection");
                return;
            }

            string myIp = NetworkManager.CurrentIP;
            int port = 8080;

            Console.WriteLine($"Starting HTTP server on port {port}...");
            Console.WriteLine($"Open a browser on your Host PC and go to: http://{myIp}:{port}");
            Console.WriteLine("Waiting for a connection...");

            try
            {
                IPAddress localAddr = IPAddress.Parse(myIp);

                var listener = new TcpListener(localAddr, port);

                listener.Start();

                var client = listener.AcceptTcpClient();
                Console.WriteLine("\n[SUCCESS] Client connected! Reading request...");

                var stream = client.GetStream();

                string html = "<html><body>" +
                              "<h1>Hello from Shinx OS!</h1>" +
                              "<p>We have successfully hijacked System.Net.Sockets!</p>" +
                              "</body></html>";

                string header = "HTTP/1.1 200 OK\r\n" +
                                "Content-Type: text/html\r\n" +
                                "Content-Length: " + html.Length + "\r\n" +
                                "Connection: close\r\n\r\n";

                string fullResponse = header + html;

                byte[] responseBytes = new byte[fullResponse.Length];
                for (int i = 0; i < fullResponse.Length; i++)
                {
                    responseBytes[i] = (byte)fullResponse[i];
                }

                stream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Webpage sent successfully.");

                stream.Close();
                client.Close();
                listener.Stop();

                Console.WriteLine("Server shut down cleanly.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Server error: {e.Message}");
            }
        }
    }
}