namespace Shared.Azure.Models;

public class IotHubKeyModel
{
    public string HostName { get; set; } = null!;
    public string SharedAccessKeyName { get; set; } = null!;
    public string SharedAccessKey { get; set; } = null!;
    public string? ConnectionString => $"HostName={HostName};SharedAccessKeyName={SharedAccessKeyName};SharedAccessKey={SharedAccessKey}";
}
