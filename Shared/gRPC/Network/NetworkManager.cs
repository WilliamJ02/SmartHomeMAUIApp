using Grpc.Net.Client;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Shared.gRPC.Network;

public class NetworkManager
{
    public List<(string IpAddress, IPAddress BroadcastAddress)> GetPrivateNetworkInterfaces()
    {
        List<(string IpAddress, IPAddress BroadcastAddress)> privateNetworks = [];

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                var ipProperties = ni.GetIPProperties();
                foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                {
                    if (
                        ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                        (ip.Address.ToString().StartsWith("192.168.") ||
                          ip.Address.ToString().StartsWith("172.16.") ||
                          ip.Address.ToString().StartsWith("172.17.") ||
                          ip.Address.ToString().StartsWith("172.18.") ||
                          ip.Address.ToString().StartsWith("172.19.") ||
                          ip.Address.ToString().StartsWith("172.20.") ||
                          ip.Address.ToString().StartsWith("172.21.") ||
                          ip.Address.ToString().StartsWith("172.22.") ||
                          ip.Address.ToString().StartsWith("172.23.") ||
                          ip.Address.ToString().StartsWith("172.24.") ||
                          ip.Address.ToString().StartsWith("172.25.") ||
                          ip.Address.ToString().StartsWith("172.26.") ||
                          ip.Address.ToString().StartsWith("172.27.") ||
                          ip.Address.ToString().StartsWith("172.28.") ||
                          ip.Address.ToString().StartsWith("172.29.") ||
                          ip.Address.ToString().StartsWith("172.30.") ||
                          ip.Address.ToString().StartsWith("172.31.") ||
                          ip.Address.ToString().StartsWith("10.")
                        )
                    )
                    {
                        var maskBytes = ip.IPv4Mask.GetAddressBytes();
                        var ipBytes = ip.Address.GetAddressBytes();

                        byte[] broadcastBytes = new byte[ipBytes.Length];
                        for (int i = 0; i < ipBytes.Length; i++)
                        {
                            broadcastBytes[i] = (byte)(ipBytes[i] | maskBytes[i] ^ 255);
                        }

                        privateNetworks.Add((ip.Address.ToString(), new IPAddress(broadcastBytes)));
                    }
                }
            }
        }

        return privateNetworks;
    }

    public async Task BroadcastOnAllPrivateNetworksAsync(CancellationToken cancellationToken)
    {
        UdpClient udpClient = new()
        {
            EnableBroadcast = true
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            var networkInterfaces = GetPrivateNetworkInterfaces();

            foreach (var (ipAddress, broadcastAddress) in networkInterfaces)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(ipAddress);
                    IPEndPoint ep = new(broadcastAddress, 9876);

                    await udpClient.SendAsync(data, data.Length, ep);
                    Debug.WriteLine($"Broadcasting: {ipAddress} on {broadcastAddress}");
                }
                catch { }
            }

            await Task.Delay(2000, cancellationToken);
        }

        udpClient.Dispose();
    }

    public async Task<string> ListenForBroadcastAsync(CancellationToken cancellationToken)
    {
        using UdpClient udpClient = new(9876);
        udpClient.EnableBroadcast = true;

        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await udpClient.ReceiveAsync();
            string ip = Encoding.UTF8.GetString(result.Buffer);

            return ip;
        }

        return null!;
    }
}
