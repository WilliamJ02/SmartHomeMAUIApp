using Grpc.Core;
using Shared.Azure.Communications;

namespace Shared.gRPC;
public class IotHubGrpcService(AzureResourceManager azureRM) : IotService.IotServiceBase
{
    private AzureResourceManager _azureRM = azureRM;

    public override async Task<IotHubInfoResponse> GetIotHubInfo(IotHubInfoRequest request, ServerCallContext context)
    {
        var result = await _azureRM.GetIotHubInfoAsync();

        return new IotHubInfoResponse
        {
            Hostname = result.HostName
        };
    }
}
