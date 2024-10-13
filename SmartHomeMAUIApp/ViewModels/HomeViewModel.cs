using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Handlers;
using Shared.Models;

namespace SmartHomeMAUIApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly AppSettingsService _appSettings;
    private readonly IotHub? _iotHub;
    public Timer? Timer { get; set; }
    public int TimerInterval { get; set; } = 4000;
    public IotHub? IotHubInstance => _iotHub;

    public HomeViewModel(IotHub iotHub, AppSettingsService appSettings)
    {
        _appSettings = appSettings;
        _iotHub = iotHub;

        _iotHub.SetConnectionString(_appSettings.ConnectionString);
    }


    public async Task<IEnumerable<LampDevice>> GetDevicesAsync()
    {
        return await _iotHub?.GetDevicesAsync() ?? Enumerable.Empty<LampDevice>();
    }
}
