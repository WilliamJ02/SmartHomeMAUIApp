using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Azure.Communications;
using Shared.Database;

namespace DeviceRemovedAzureFunctions;

public class DeviceRemovedFunction
{
    private readonly ILogger<DeviceRemovedFunction> _logger;
    private readonly EmailCommunication _emailCommunication;
    private readonly DatabaseService _databaseService;

    public DeviceRemovedFunction(ILogger<DeviceRemovedFunction> logger, EmailCommunication emailCommunication, DatabaseService databaseService)
    {
        _logger = logger;
        _emailCommunication = emailCommunication;
        _databaseService = new DatabaseService();
    }

    [Function(nameof(DeviceRemovedFunction))]
    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        try
        {
            _logger.LogInformation("Event type: {eventType}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);
            
            if (eventGridEvent.EventType == "Microsoft.Devices.DeviceDeleted")
            {
                var settings = await _databaseService.GetSettingsAsync();
                if (settings != null)
                {
                    string toAddress = settings.EmailAddress;
                    string subject = "Device Removed";
                    string bodyHtml = $"<p>The device with ID <strong>{eventGridEvent.Subject}</strong> has been removed.</p>";
                    string bodyPlainText = $"The device with ID {eventGridEvent.Subject} has been removed.";

                    _emailCommunication.Send(toAddress, subject, bodyHtml, bodyPlainText);

                    _logger.LogInformation("Email sent successfully to {email} for device removal.", toAddress);
                }
                else
                {
                    _logger.LogWarning("No email address found in the SQLite database.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the EventGridEvent or sending email.");
            throw;
        }
    }
}
