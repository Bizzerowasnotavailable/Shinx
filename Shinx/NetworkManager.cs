using System;
using System.Collections.Generic;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Timers;

namespace Shinx
{
    public static class NetworkManager
    {
        public static bool IsConnected { get; private set; } = false;
        public static string CurrentIP { get; private set; } = "0.0.0.0";
        public static string CurrentMask { get; private set; } = "0.0.0.0";
        public static string CurrentGateway { get; private set; } = "0.0.0.0";
        public static string Mode { get; private set; } = "none";
        public static string DNSServer { get; private set; } = "1.1.1.1";

        private static readonly string configFile = "/sys/network.txt";

        public static void Init()
        {
            if (Cosmos.Kernel.System.Network.NetworkManager.DeviceCount == 0)
            {
                Console.WriteLine("[net] No network devices found.");
                return;
            }

            LoadConfig();

            if (Mode == "static")
            {
                SetStatic(CurrentIP, CurrentMask, CurrentGateway);
            }
            else if (Mode == "dhcp")
            {
                SetDHCP();
            }
            else
            {
                Console.WriteLine("[net] System started without network configuration.");
            }
        }

        public static void SetDHCP()
        {
            try
            {
                Console.WriteLine("[net] Discovering DHCP server...");
                DhcpClient dhcpClient = new DhcpClient();
                if (dhcpClient.SendDiscoverPacket() != -1)
                {
                    var cfg = Cosmos.Kernel.System.Network.NetworkManager.Primary.IPConfig;
                    if (cfg != null)
                    {
                        CurrentIP = cfg.Address.ToString();
                        CurrentMask = cfg.SubnetMask.ToString();
                        CurrentGateway = cfg.DefaultGateway.ToString();
                        IsConnected = true;
                        Mode = "dhcp";
                        SaveConfig();
                        Console.WriteLine($"[net] Success! IP: {CurrentIP}");
                        return;
                    }
                }
                Console.WriteLine("[net] DHCP Timeout (No response from router).");
                IsConnected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("[net] DHCP Error: " + e.Message);
                IsConnected = false;
            }
        }

        public static void SetStatic(string ip, string mask, string gateway)
        {
            try
            {
                if (Cosmos.Kernel.System.Network.NetworkManager.DeviceCount == 0)
                {
                    Console.WriteLine("[net] No network device.");
                    return;
                }

                var a1 = System.Net.IPAddress.Parse(ip);
                var b1 = a1.GetAddressBytes();
                var a2 = System.Net.IPAddress.Parse(mask);
                var b2 = a2.GetAddressBytes();
                var a3 = System.Net.IPAddress.Parse(gateway);
                var b3 = a3.GetAddressBytes();
                if (!IPConfig.Enable(new Address4(b1[0], b1[1], b1[2], b1[3]),
                    new Address4(b2[0], b2[1], b2[2], b2[3]),
                    new Address4(b3[0], b3[1], b3[2], b3[3])))
                {
                    Console.WriteLine("[net] Failed to enable IP config.");
                    return;
                }

                CurrentIP = ip;
                CurrentMask = mask;
                CurrentGateway = gateway;
                IsConnected = true;
                Mode = "static";

                SaveConfig();
                Console.WriteLine($"[net] Static IP {ip} enabled.");
            }
            catch (Exception e)
            {
                Console.WriteLine("[net] Static setup failed: " + e.Message);
            }
        }

        public static void Status()
        {
            if (!IsConnected)
            {
                Console.WriteLine("Network: Disconnected");
                return;
            }
            Console.WriteLine($"Status:  Connected ({Mode})");
            Console.WriteLine($"IP:      {CurrentIP}");
            if (Mode == "static")
            {
                Console.WriteLine($"Mask:    {CurrentMask}");
                Console.WriteLine($"Gateway: {CurrentGateway}");
            }
            Console.WriteLine($"DNS:     {DNSServer}");
        }

        public static void SetDNS(string dns)
        {
            try
            {
                var parts = dns.Split('.');
                DnsConfig.Add(new Address4(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]), byte.Parse(parts[3])));
                DNSServer = dns;
                SaveConfig();
                Console.WriteLine($"[net] DNS set to {dns}");
            }
            catch { Console.WriteLine("[net] Invalid DNS IP."); }
        }

        public static string Resolve(string hostname)
        {
            if (!IsConnected) return null;

            string norm = hostname.Replace("https://", "").Replace("http://", "");

            try
            {
                try
                {
                    var addrs = System.Net.Dns.GetHostAddresses(norm);
                    if (addrs.Length > 0) return addrs[0].ToString();
                }
                catch { }

                var dnsClient = new DnsClient();
                var dp = DNSServer.Split('.');
                dnsClient.Connect(new Address4(byte.Parse(dp[0]), byte.Parse(dp[1]), byte.Parse(dp[2]), byte.Parse(dp[3])));
                dnsClient.SendQuery(norm);
                var result = dnsClient.Receive(5000);
                return result?.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("[dns] Failed: " + e.Message);
                return null;
            }
        }
        public static List<string> ResolveAll(string hostname)
        {
            if (!IsConnected) return null;

            string norm = hostname.Replace("https://", "").Replace("http://", "");

            try
            {
                try
                {
                    var addrs = System.Net.Dns.GetHostAddresses(norm);
                    if (addrs != null && addrs.Length > 0)
                    {
                        var addresses = new List<string>();
                        foreach (var addr in addrs)
                            addresses.Add(addr.ToString());
                        return addresses;
                    }
                }
                catch { }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("[dns] Failed: " + e.Message);
                return null;
            }
        }

        private static void SaveConfig()
        {
            try
            {
                if (!Directory.Exists("/sys")) Directory.CreateDirectory("/sys");
                File.WriteAllLines(configFile, new[]
                {
                    $"mode={Mode}",
                    $"ip={CurrentIP}",
                    $"mask={CurrentMask}",
                    $"gateway={CurrentGateway}",
                    $"dns={DNSServer}"
                });
            }
            catch { }
        }

        private static void LoadConfig()
        {
            if (!File.Exists(configFile)) return;
            try
            {
                foreach (var line in File.ReadAllLines(configFile))
                {
                    var p = line.Split('=');
                    if (p.Length != 2) continue;
                    string val = p[1].Trim();
                    switch (p[0].Trim())
                    {
                        case "mode": Mode = val; break;
                        case "ip": CurrentIP = val; break;
                        case "mask": CurrentMask = val; break;
                        case "gateway": CurrentGateway = val; break;
                        case "dns": DNSServer = val; break;
                    }
                }
            }
            catch { }
        }
        public static void Disconnect()
        {
            IsConnected = false;
            Mode = "none";
            CurrentIP = "0.0.0.0";
            SaveConfig();
            Console.WriteLine("network: disconnected");
        }
    }
}
