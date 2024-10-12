using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Shared.Azure.Communications;

namespace Shared.gRPC;

public class GrpcManager(AzureResourceManager azureRM)
{
    private readonly AzureResourceManager _azureRM = azureRM;

    public async Task StartAsync(string connectionString)
    {
        const int Port = 50051;
        Server server = new()
        {
            Services = { IotService.BindService(new IotHubGrpcService(_azureRM)) },
            Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
        };

        server.Start();
        await Task.Delay(Timeout.Infinite);
    }
}
