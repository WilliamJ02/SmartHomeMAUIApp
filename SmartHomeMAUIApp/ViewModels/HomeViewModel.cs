using CommunityToolkit.Mvvm.ComponentModel;
using Shared.Handlers;
using Shared.Models;
using System.Collections.ObjectModel;

namespace SmartHomeMAUIApp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private IotHub? _iotHub;
    public Timer? Timer { get; set; }
    public int TimerInterval { get; set; } = 4000;

    public HomeViewModel(IotHub iotHub)
    {
        _iotHub = iotHub;
    }
    public async Task<IEnumerable<LampDevice>> GetDevicesAsync()
    {
        return await _iotHub.GetDevicesAsync();
    }
}
