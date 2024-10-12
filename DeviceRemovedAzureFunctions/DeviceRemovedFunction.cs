using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.Azure.Communications;

namespace DeviceRemovedAzureFunctions;

public class DeviceRemovedFunction
{
    private readonly ILogger<DeviceRemovedFunction> _logger;
    private readonly EmailCommunication _emailCommunication;

    public DeviceRemovedFunction(ILogger<DeviceRemovedFunction> logger, EmailCommunication emailCommunication)
    {
        _logger = logger;
        _emailCommunication = emailCommunication;
    }

    [Function(nameof(DeviceRemovedFunction))]
    public void Run([EventGridTrigger] EventGridEvent eventGridEvent)
    {
        try
        {
            _logger.LogInformation("Event type: {eventType}, Event subject: {subject}", eventGridEvent.EventType, eventGridEvent.Subject);

            if (eventGridEvent.EventType == "Microsoft.Devices.DeviceDeleted")
            {
                string deviceId = eventGridEvent.Subject;

                string toAddress = "william.jarnebrant@hotmail.com";  
                string subject = $"Device Removed: {deviceId}";
                string bodyHtml = $"<p>The device with ID <strong>{deviceId}</strong> has been removed.</p>";
                string bodyPlainText = $"The device with ID {deviceId} has been removed.";

                _emailCommunication.Send(toAddress, subject, bodyHtml, bodyPlainText);

                _logger.LogInformation("Email sent successfully for device removal.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the EventGridEvent or sending email.");
            throw;
        }
    }
}
