using Cosmos.System.Network;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DNS;
using CosmosHttp.Client;
using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    internal class core_wget : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: wget <domain> (e.g., wget httpforever.com)");
                return;
            }

            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("wget: no network connection");
                return;
            }

            string target = args[0].Trim();

            if (target.StartsWith("http://")) target = target.Substring(7);
            if (target.StartsWith("https://"))
            {
                Console.WriteLine("wget: HTTPS is not supported yet. Please use standard HTTP.");
                return;
            }

            string domain = target;
            string path = "/";

            if (target.Contains("/"))
            {
                int slashIdx = target.IndexOf("/");
                domain = target.Substring(0, slashIdx);
                path = target.Substring(slashIdx);
            }

            Console.WriteLine($"Resolving {domain}...");

            try
            {
                var dnsClient = new DnsClient();
                dnsClient.Connect(DNSConfig.DNSNameservers[0]);
                dnsClient.SendAsk(domain);
                Address address = dnsClient.Receive();
                dnsClient.Close();

                if (address == null)
                {
                    Console.WriteLine($"wget: failed to resolve IP for {domain}");
                    return;
                }

                Console.WriteLine($"Connecting to {domain} ({address})...");

                HttpRequest request = new();
                request.IP = address.ToString();
                request.Domain = domain;
                request.Path = path;
                request.Method = "GET";

                request.Send();

                Console.WriteLine("\n--- WEBPAGE DOWNLOADED ---");
                Console.WriteLine(request.Response.Content);
                Console.WriteLine("--------------------------");
            }
            catch (Exception e)
            {
                Console.WriteLine($"wget error: {e.Message}");
            }
        }
    }
}