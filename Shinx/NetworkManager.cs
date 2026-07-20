using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.UDP.DHCP;
using Cosmos.Kernel.System.Network.IPv4.UDP.DNS;
using Cosmos.Kernel.System.Timer;
using System;
using System.IO;
using System.Collections.Generic;

namespace Shinx
{
    public static class NetworkManager
    {
        public static bool IsConnected { get; private set; } = false;
        public static string CurrentIP { get; private set; } = "0.0.0.0";
        public static string CurrentMask { get; private set; } = "0.0.0.0";
        public static string CurrentGateway { get; private set; } = "0.0.0.0";
        public static string Mode { get; private set; } = "none";
        public static string DNSServer { get; private set; } = "8.8.8.8";

        private static readonly string configFile = "/sys/network.txt";

        public static void Init()
        {
            var device = Cosmos.Kernel.System.Network.NetworkManager.PrimaryDevice;

            if (device == null)
            {
                Console.WriteLine("[net] No network devices found.");
                return;
            }

            LoadConfig();

            // Initialize the network stack before using DHCP/DNS
            Cosmos.Kernel.System.Network.NetworkStack.Initialize();

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

                using (var xClient = new DHCPClient())
                {
                    xClient.SendDiscoverPacket();
                }

                int maxAttempts = 20;
                var netDevice = Cosmos.Kernel.System.Network.NetworkManager.PrimaryDevice;
                while (Cosmos.Kernel.System.Network.Config.NetworkConfigManager.Get(netDevice) == null && maxAttempts > 0)
                {
                    TimerManager.Wait(500);
                    maxAttempts--;
                }

                if (Cosmos.Kernel.System.Network.Config.NetworkConfigManager.Get(netDevice) != null)
                {
                    UpdateStatus();
                    IsConnected = true;
                    Mode = "dhcp";
                    SaveConfig();
                    Console.WriteLine($"[net] Success! IP: {CurrentIP}");
                }
                else
                {
                    Console.WriteLine("[net] DHCP Timeout (No response from router).");
                    IsConnected = false;
                }
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
                var nic = Cosmos.Kernel.System.Network.NetworkManager.PrimaryDevice;

                IPConfig.Enable(nic, Address.Parse(ip), Address.Parse(mask), Address.Parse(gateway));

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
                Address.Parse(dns);
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
                using (var dnsClient = new DnsClient())
                {
                    dnsClient.Connect(Address.Parse(DNSServer));
                    dnsClient.SendAsk(norm);
                    Address result = dnsClient.Receive();
                    return result?.ToString();
                }
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
                using (var dnsClient = new DnsClient())
                {
                    dnsClient.Connect(Address.Parse(DNSServer));
                    dnsClient.SendAsk(norm);
                    List<Address> result = dnsClient.ReceiveAll();
                    if (result == null) return null;
                    var addresses = new List<string>();
                    foreach (var addr in result)
                        addresses.Add(addr.ToString());
                    return addresses;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[dns] Failed: " + e.Message);
                return null;
            }
        }

        private static void UpdateStatus()
        {
            var device = Cosmos.Kernel.System.Network.NetworkManager.PrimaryDevice;
            var config = Cosmos.Kernel.System.Network.Config.NetworkConfigManager.Get(device);
            if (config != null)
                CurrentIP = config.IPAddress.ToString();
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