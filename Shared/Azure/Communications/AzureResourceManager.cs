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
    ResourceGroupResource _currentResourceGroup = null!;
    IotHubDescriptionResource _currentIotHub = null!;

    public async Task InitializeAsync()
    {
        _client = new(new DefaultAzureCredential());
        _subscription = await _client.GetDefaultSubscriptionAsync();
    }

    public async Task<ResourceGroupResource> CreateResourceGroupAsync(string resourceGroupName, AzureLocation location)
    {
        try
        {
            var loc = location;

            ResourceGroupCollection resourceGroups = _subscription.GetResourceGroups();
            ResourceGroupData resourceGroupData = new(loc);
            ArmOperation<ResourceGroupResource> operation = await resourceGroups.CreateOrUpdateAsync(WaitUntil.Completed, resourceGroupName, resourceGroupData);

            _currentResourceGroup = operation.Value;
            return operation.Value;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubDescriptionResource> CreateIotHubAsync(string iotHubUniqueName, string sku)
    {
        try
        {
            IotHubDescriptionCollection iotHubDescriptionCollection = _currentResourceGroup.GetIotHubDescriptions();
            IotHubDescriptionData iotHubDescriptionData = new(AzureLocation.WestEurope, new IotHubSkuInfo(GetIotHubSku(sku)) { Capacity = 1 });
            ArmOperation<IotHubDescriptionResource> operation = await iotHubDescriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, iotHubUniqueName, iotHubDescriptionData);

            _currentIotHub = operation.Value;
            return operation.Value;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubDescriptionResource> CreateIotHubAsync(ResourceGroupResource resourceGroup, string iotHubUniqueName, string sku)
    {
        try
        {
            IotHubDescriptionCollection iotHubDescriptionCollection = resourceGroup.GetIotHubDescriptions();
            IotHubDescriptionData iotHubDescriptionData = new(AzureLocation.WestEurope, new IotHubSkuInfo(GetIotHubSku(sku)) { Capacity = 1 });
            ArmOperation<IotHubDescriptionResource> operation = await iotHubDescriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, iotHubUniqueName, iotHubDescriptionData);

            _currentIotHub = operation.Value;
            return operation.Value;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubKeyModel> GetIotHubConnectionStringAsync(string keyName = "iothubowner")
    {
        try
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
        catch
        {
            return null!;
        }
    }

    public async Task<IotHubKeyModel> GetIotHubConnectionStringAsync(IotHubDescriptionResource iotHub, string keyName = "iothubowner")
    {
        try
        {
            var result = await _currentIotHub.GetKeysForKeyNameAsync(keyName);
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
