using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class LinkedDevice
{
    public string IP { get; set; }
    public int Port { get; set; }
    public Socket Socket { get; set; }
}

public class AppContext
{
    public List<LinkedDevice> LinkedDevices { get; } = new List<LinkedDevice>();
    public int LinkedCount => LinkedDevices.Count;
}

public class Scanner
{
    public const int PORT = 60000;
    public const int TIMEOUT_MS = 1000;
    public const int MAX_DEVICES = 64;

    public static string GetLocalSubnet()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                if (ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ua.Address))
                {
                    var bytes = ua.Address.GetAddressBytes();
                    return $"{bytes[0]}.{bytes[1]}.{bytes[2]}";
                }
            }
        }
        return null;
    }

    public static async Task ScanSubnetAsync(AppContext ctx, CancellationToken ct)
    {
        string baseIp = GetLocalSubnet();
        if (baseIp == null)
        {
            Console.WriteLine("Could not determine local subnet.");
            return;
        }

        var tasks = new List<Task>();
        var locker = new object();

        for (int i = 1; i <= 254 && ctx.LinkedDevices.Count < MAX_DEVICES; i++)
        {
            string ip = $"{baseIp}.{i}";
            tasks.Add(Task.Run(() =>
            {
                using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        var result = client.BeginConnect(ip, PORT, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(TIMEOUT_MS);
                        if (success && client.Connected)
                        {
                            lock (locker)
                            {
                                if (ctx.LinkedDevices.Count < MAX_DEVICES)
                                {
                                    ctx.LinkedDevices.Add(new LinkedDevice
                                    {
                                        IP = ip,
                                        Port = PORT,
                                        Socket = client
                                    });
                                    // Do NOT dispose here since you keep the socket
                                }
                                else
                                {
                                    client.Close();
                                }
                            }
                        }
                        else
                        {
                            client.Close();
                        }
                    }
                    catch { /* Ignore failures */ }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);
    }
}

// Usage example:
class Program
{
    static async Task Main()
    {
        var ctx = new AppContext();
        var cts = new CancellationTokenSource();

        Console.WriteLine("Scanning subnet...");
        await Scanner.ScanSubnetAsync(ctx, cts.Token);

        Console.WriteLine($"Scan complete. Found {ctx.LinkedCount} device(s):");
        foreach (var dev in ctx.LinkedDevices)
            Console.WriteLine($"{dev.IP}:{dev.Port}");

        // Example: Connect to first found device, if needed
        if (ctx.LinkedDevices.Any())
        {
            var first = ctx.LinkedDevices.First();
            Console.WriteLine($"Auto-connected to {first.IP}:{first.Port}");
            // Call your RFID connection method here
        }
        else
        {
            Console.WriteLine("No devices found.");
        }
    }
}

