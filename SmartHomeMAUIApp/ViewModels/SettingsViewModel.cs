﻿using Azure.Communication.Email;
using Azure.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Azure.Communications;
using Shared.Database;
using Shared.gRPC;
using Shared.Handlers;

namespace SmartHomeMAUIApp.ViewModels;

public partial class SettingsViewModel : ObservableObject 
{
    private readonly EmailCommunication _email;
    private readonly DatabaseService _database;
    private readonly AzureResourceManager _azureRM;
    private readonly GrpcManager _grpc;
    private readonly AppSettingsService _appSettings;
    public SettingsViewModel(EmailCommunication email, DatabaseService database, AzureResourceManager azureRM, GrpcManager grpc, AppSettingsService appSettingsService)
    {
        _email = email;
        _database = database;
        _azureRM = azureRM;
        _grpc = grpc;
        _appSettings = appSettingsService;

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

    public void SaveSettings()
    {
        _appSettings.ConnectionString = ConnectionString;
    }
}
