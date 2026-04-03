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

            string host = args[0].Trim();
            int count = 4;
            string ip = host;

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