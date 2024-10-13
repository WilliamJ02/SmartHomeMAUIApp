using Shared.Handlers;

var iotHub = new IotHub();
var connectionString = "HostName=iothub-4ed0b35a.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=5zMcO7EU2LT/LyTeAIyhpAncqW10lX/FTAIoTDNdPtw=";

if (!iotHub.SetConnectionString(connectionString))
{
    Console.WriteLine("Failed to set connection string.");
    return;
}

var previousDeviceIds = new HashSet<string>();

while (true)
{
    var currentDevices = await iotHub.GetDevicesAsync();
    var currentDeviceIds = new HashSet<string>(currentDevices.Select(d => d.DeviceId));

    foreach (var deviceId in previousDeviceIds)
    {
        if (!currentDeviceIds.Contains(deviceId))
        {
            Console.WriteLine($"Device '{deviceId}' has been deleted.");
        }
    }

    previousDeviceIds = currentDeviceIds;

    await Task.Delay(10000);
}