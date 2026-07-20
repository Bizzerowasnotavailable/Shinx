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
                Console.WriteLine("usage: net <command>");
                Console.WriteLine("  net status");
                Console.WriteLine("  net dhcp");
                Console.WriteLine("  net static <ip> <mask> <gateway>");
                Console.WriteLine("  net dns <ip>");
                Console.WriteLine("  net resolve <hostname>");
                Console.WriteLine("  net disconnect");
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
                    List<string> ip = NetworkManager.ResolveAll(args[1]);
                    if (ip != null)
                    {
                        foreach (var addr in ip)
                            Console.WriteLine($"{args[1]} -> {addr}");
                    }
                    break;

                case "disconnect":
                    NetworkManager.Disconnect();
                    break;

                default:
                    Console.WriteLine($"net: unknown command: {args[0]}");
                    break;
            }
        }
    }
}