using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.IotHub;
using Azure.ResourceManager.IotHub.Models;
using Azure.ResourceManager.Resources;
using Shared.Azure.Models;

namespace Shared.Azure.Communications;

public class AzureResourceManager
{
    ArmClient _client = null!;
    SubscriptionResource _subscription = null!;
    ResourceGroupResource? _currentResourceGroup;
    IotHubDescriptionResource? _currentIotHub;

    public async Task InitializeAsync()
    {
        try
        {
            var defaultSubscriptionId = "c6aef30e-b724-47f9-9cb4-7e98797aaeb4";
            var tenantId = "5c2a06ee-4772-48db-90c3-d69f8666c5bc";
            _client = new ArmClient(new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = tenantId }));

            _subscription = _client.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{defaultSubscriptionId}"));

            var subscriptionDetails = await _subscription.GetAsync();
        }
        catch (Exception ex)
        {

        }
    }

    public async Task<ResourceGroupResource> CreateResourceGroupAsync(string resourceGroupName, string location)
    {
        try
        {
            ResourceGroupCollection resourceGroups = _subscription.GetResourceGroups();
            ResourceGroupData resourceGroupData = new(GetAzureLocation(location));
            ArmOperation<ResourceGroupResource> operation = await resourceGroups.CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, resourceGroupData);

            _currentResourceGroup = operation.Value;
            return operation.Value;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubDescriptionResource> CreateIotHubAsync(string iotHubUniqueName, string location, string sku)
    {
        try
        {
            IotHubDescriptionCollection iotHubDescriptionCollection = _currentResourceGroup.GetIotHubDescriptions();
            IotHubDescriptionData iotHubDescriptionData = new(location, new IotHubSkuInfo(GetIotHubSku(sku)) { Capacity = 1 });
            ArmOperation<IotHubDescriptionResource> operation = await iotHubDescriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, iotHubUniqueName, iotHubDescriptionData);

            _currentIotHub = operation.Value;
            return operation.Value;
        }
        
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubDescriptionResource> CreateIotHubAsync(ResourceGroupResource resourceGroup, string iotHubUniqueName, string location, string sku)
    {
        try
        {
            IotHubDescriptionCollection iotHubDescriptionCollection = resourceGroup.GetIotHubDescriptions();
            IotHubDescriptionData iotHubDescriptionData = new(location, new IotHubSkuInfo(GetIotHubSku(sku)) { Capacity = 1 });
            ArmOperation<IotHubDescriptionResource> operation = await iotHubDescriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, iotHubUniqueName, iotHubDescriptionData);

            _currentIotHub = operation.Value;
            return operation.Value;
        }
        catch (Exception ex)
        {
            return null!;
        }
    }

    public async Task<IotHubKeyModel> GetIotHubInfoAsync(string keyName = "iothubowner")
    {
        try
        {
            if (_currentIotHub != null)
            {
                var result = await _currentIotHub.GetKeysForKeyNameAsync(keyName);
                var value = result.Value;

                var iotHubKeyModel = new IotHubKeyModel()
                {
                    HostName = _currentIotHub.Data.Name,
                    SharedAccessKeyName = value.KeyName,
                    SharedAccessKey = value.PrimaryKey
                };

                return iotHubKeyModel;
            }
            return null!;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubKeyModel> GetIotHubConnectionStringAsync(IotHubDescriptionResource iotHub, string keyName = "iothubowner")
    {
        try
        {
            var result = await iotHub.GetKeysForKeyNameAsync(keyName);
            var value = result.Value;

            var iotHubKeyModel = new IotHubKeyModel()
            {
                HostName = $"{iotHub.Data.Name}.azure-devices.net",
                SharedAccessKeyName = value.KeyName,
                SharedAccessKey = value.PrimaryKey
            };

            return iotHubKeyModel;
        }
        catch
        {
            return null!;
        }
    }

    public AzureLocation GetAzureLocation(string location)
    {
        return location.ToLower() switch
        {
            "westeurope" => AzureLocation.WestEurope,
            "northeurope" => AzureLocation.NorthEurope,
            "swedencentral" => AzureLocation.SwedenCentral,
            _ => AzureLocation.WestEurope,
        };
    }

    public static IotHubSku GetIotHubSku(string sku)
    {
        return sku.ToUpper() switch
        {
            "F1" => IotHubSku.F1,
            "S1" => IotHubSku.S1,
            _ => IotHubSku.S1
        };
    }
}
