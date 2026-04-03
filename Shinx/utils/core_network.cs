using System;
using System.Collections.Generic;

namespace Shinx.Commands
{
    internal class core_network : ICommand
    {
        public void Execute(string[] args, HashSet<char> parameters)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: network <command>");
                Console.WriteLine("  network status");
                Console.WriteLine("  network dhcp");
                Console.WriteLine("  network static <ip> <mask> <gateway>");
                Console.WriteLine("  network dns <ip>");
                Console.WriteLine("  network resolve <hostname>");
                Console.WriteLine("  network disconnect");
                return;
            }

            switch (args[0])
            {
                case "status":
                    NetworkManager.Status();
                    break;

                case "dhcp":
                    NetworkManager.SetDHCP();
                    break;

                case "static":
                    if (args.Length < 4)
                    {
                        Console.WriteLine("usage: network static <ip> <mask> <gateway>");
                        return;
                    }
                    NetworkManager.SetStatic(args[1], args[2], args[3]);
                    break;

                case "dns":
                    if (args.Length < 2)
                    {
                        Console.WriteLine($"current dns: {NetworkManager.DNSServer}");
                        return;
                    }
                    NetworkManager.SetDNS(args[1]);
                    break;

                case "resolve":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("usage: network resolve <hostname>");
                        return;
                    }
                    string ip = NetworkManager.Resolve(args[1]);
                    if (ip != null)
                        Console.WriteLine($"{args[1]} -> {ip}");
                    break;

                case "disconnect":
                    NetworkManager.Disconnect();
                    break;

                default:
                    Console.WriteLine($"network: unknown command: {args[0]}");
                    break;
            }
        }
    }
}