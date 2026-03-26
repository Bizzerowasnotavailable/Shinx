using Cosmos.System.Network.IPv4;
using Cosmos.System.Network;
using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    internal class core_ping : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: ping <host>");
                return;
            }

            if (!NetworkManager.IsConnected)
            {
                Console.WriteLine("ping: no network connection");
                return;
            }

            // 1. Trim cleans any invisible characters from the console (Fixes the raw IP bug)
            string host = args[0].Trim();
            int count = 4;
            string ip = host;

            // 2. If it doesn't look like a raw IP address, ask the DNS server to resolve it
            if (host.Split('.').Length != 4)
            {
                ip = NetworkManager.Resolve(host);
                if (ip == null)
                {
                    Console.WriteLine($"ping: could not resolve {host}");
                    return;
                }
            }

            Console.WriteLine($"pinging {host} ({ip})...");

            try
            {
                // Let Cosmos handle the safe parsing
                var target = Address.Parse(ip);

                int sent = 0, received = 0;
                var endpoint = new EndPoint(Address.Zero, 0);

                using (var icmp = new ICMPClient())
                {
                    icmp.Connect(target);

                    for (int i = 0; i < count; i++)
                    {
                        icmp.SendEcho();
                        sent++;

                        // FORCE Cosmos to wait up to 4000ms (4 seconds) for the reply
                        int time = icmp.Receive(ref endpoint, 4000);

                        if (time == -1)
                        {
                            Console.WriteLine("  request timeout");
                        }
                        else
                        {
                            received++;
                            Console.WriteLine($"  reply from {ip}: time={time}ms");
                        }

                        // CRITICAL: Wait 1 second before sending the next ping.
                        // This prevents the Cosmos network buffer from flooding.
                        if (i < count - 1)
                        {
                            Cosmos.HAL.Global.PIT.Wait(1000);
                        }
                    }
                }

                int lost = sent - received;
                int lossPercent = (lost * 100) / sent;
                Console.WriteLine($"  sent={sent} received={received} lost={lost} ({lossPercent}% loss)");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ping error: {e.Message}");
            }
        }
    }
}