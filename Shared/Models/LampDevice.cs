namespace Shared.Models;

public class LampDevice
{
	public bool ConnectionState { get; set; }
	public string DeviceId { get; set; } = null!;
	public string? DeviceName { get; set; } = "";
	public string? DeviceType { get; set; } = "";



	public event Action<bool>? DeviceStateChanged;
	private bool deviceState;

	public bool DeviceState
	{
		get => deviceState;
		set
		{
			if (deviceState == value) return;
			deviceState = value;

			DeviceStateChanged?.Invoke(deviceState);
		}
	}
}
