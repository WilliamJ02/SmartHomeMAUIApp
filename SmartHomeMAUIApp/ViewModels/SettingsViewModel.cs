using Azure.Communication.Email;
using Azure.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Azure.Communications;
using Shared.Database;
using Shared.gRPC;
using Shared.Handlers;
using System.Threading;

namespace SmartHomeMAUIApp.ViewModels;

public partial class SettingsViewModel : ObservableObject 
{
    private readonly EmailCommunication _email;
    private readonly DatabaseService _database;
    private readonly AzureResourceManager _azureRM;
    private readonly GrpcManager _grpc;
    private readonly AppSettingsService _appSettings;
    private readonly IotHub? _iotHub;
    private CancellationTokenSource? _cancellationTokenSource;
    public SettingsViewModel(EmailCommunication email, DatabaseService database, AzureResourceManager azureRM, GrpcManager grpc, AppSettingsService appSettingsService, IotHub? iotHub)
    {
        _email = email;
        _database = database;
        _azureRM = azureRM;
        _grpc = grpc;
        _appSettings = appSettingsService;
        _iotHub = iotHub;       
        
        var settings = _database.GetSettingsAsync().Result;
        
        if (settings != null)
        {
            IsConfigured = true;
            EmailInput = settings.EmailAddress;
        }
        ConfigureButton = IsConfigured ? "Configured" : "Configure";
    }

    [ObservableProperty]
    private bool _isConfigured = false;

    [ObservableProperty]
    private string _configuredText = "";

    [ObservableProperty]
    private string _emailInput = null!;

    [ObservableProperty]
    private string _connectionString;

    [ObservableProperty]
    private string _configureButton = "Configure";

    [RelayCommand]
    public async Task Configure()
    {
        try
        {
            ConfigureButton = "Configuring...";
            
            await _azureRM.InitializeAsync();
            
            IsConfigured = await ConfigureSettingsAsync();

            if (IsConfigured)
            {
                _email.Send(EmailInput, "Azure IoTHub Successfully Connected", "<h1>Your Azure IoTHub is now connected</h1>", "Your Azure IoTHub is now connected");
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex);
        }
    }

    public async Task<bool> ConfigureSettingsAsync()
    {
        try
        {
            var settings = await _database.GetSettingsAsync();
            if (settings == null)
            {
                settings = new Shared.Database.Settings()
                {
                    AppId = Guid.NewGuid().ToString().Split('-')[0],
                    EmailAddress = EmailInput
                };

                var iotHub = await _azureRM.GetIotHubInfoAsync();

                if (iotHub == null)
                {
                    await _azureRM.CreateResourceGroupAsync($"rg-{settings.AppId}", "westeurope");
                    await _azureRM.CreateIotHubAsync($"iothub-{settings.AppId}", "westeurope", "F1");
                    iotHub = await _azureRM.GetIotHubInfoAsync();
                }

                settings.IotHubConnectionString = iotHub.ConnectionString!;

                var result = await _database.SaveSettingsAsync(settings);

                return result == 1 ? true : false;
            }
        }
        catch
        {
            return false;
        }
       
        return false;
    }

    [ObservableProperty]
    private string _connectionSucceededText;

    [ObservableProperty]
    private string _connectButtonText = "Connect";

    public void SaveSettings()
    {
        _appSettings.ConnectionString = ConnectionString;

        if (_iotHub.SetConnectionString(_appSettings.ConnectionString) == true)
        {            
            ConnectionSucceededText = "Connection succeeded!";
            ConnectButtonText = "Connected";
            StartDeviceMonitoring();
        }
        else
        {
            ConnectionSucceededText = "Connection string not found.";
        }
    }

    private async void StartDeviceMonitoring()
    {
        if (_iotHub == null || !_iotHub.IsRegistryInitialized)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        await Task.Run(async () =>
        {
            var previousDeviceIds = new HashSet<string>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentDevices = await _iotHub.GetDevicesAsync();
                    var currentDeviceIds = new HashSet<string>(currentDevices.Select(d => d.DeviceId));

                    foreach (var deviceId in previousDeviceIds)
                    {
                        if (!currentDeviceIds.Contains(deviceId))
                        {
                            _email.Send(EmailInput, "IoT Device Deleted", $"<h1>IoT device with ID {deviceId} deleted. </h1>", $"IoT device with ID {deviceId} deleted.");
                        }
                    }

                    previousDeviceIds = currentDeviceIds;

                    await Task.Delay(8000, cancellationToken);
                }
                catch (Exception ex)
                {
                }
            }
        }, cancellationToken);
    }

    public void StopDeviceMonitoring()
    {
        _cancellationTokenSource?.Cancel();
    }
}
