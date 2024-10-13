using Microsoft.Azure.Devices;
using Shared.Models;

namespace Shared.Handlers
{
    public class IotHub
    {
        private string? _connectionString;
        private RegistryManager? _registry;
        public bool ConnectionSucceeded { get; private set; } = false;

        public bool SetConnectionString(string connectionString)
        {
            try
            {
                _connectionString = connectionString;
                _registry = RegistryManager.CreateFromConnectionString(_connectionString);
                ConnectionSucceeded = true;
                return true;
            }
            catch (Exception ex)
            {
                _registry = null; 
                ConnectionSucceeded = false;
                return false;
            }
        }
        public bool IsRegistryInitialized => _registry != null;
        public async Task<IEnumerable<LampDevice>> GetDevicesAsync()
        {
            if (_registry == null)
            {
                throw new InvalidOperationException("RegistryManager is not initialized. Please check the connection string.");
            }

            var query = _registry.CreateQuery("select * from devices");
            var devices = new List<LampDevice>();

            foreach (var twin in await query.GetNextAsTwinAsync())
            {
                var device = new LampDevice
                {
                    DeviceId = twin.DeviceId
                };

                try { device.DeviceName = twin?.Properties?.Reported["deviceName"]?.ToString(); }
                catch { device.DeviceName = "Unknown"; }

                try { device.DeviceType = twin?.Properties?.Reported["deviceType"]?.ToString(); }
                catch { device.DeviceType = "Unknown"; }

                try
                {
                    bool.TryParse(twin?.Properties?.Reported["connectionState"]?.ToString(), out bool connectionState);
                    device.ConnectionState = connectionState;
                }
                catch { device.ConnectionState = false; }

                if (device.ConnectionState)
                {
                    try
                    {
                        bool.TryParse(twin?.Properties?.Reported["LampToggled"]?.ToString(), out bool deviceState);
                        device.DeviceState = deviceState;
                    }
                    catch { device.DeviceState = false; }

                    try
                    {
                        int.TryParse(twin?.Properties?.Reported["LampBrightness"]?.ToString(), out int brightness);
                        device.Brightness = brightness;
                    }
                    catch
                    {
                        device.Brightness = 0; 
                    }
                }

                devices.Add(device);
            }

            return devices;
        }
    }
}
