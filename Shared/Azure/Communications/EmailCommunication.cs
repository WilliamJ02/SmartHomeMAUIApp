using Azure.Communication.Email;
using Azure;
using System.Text;

namespace Shared.Azure.Communications;

public class EmailCommunication
{
    private readonly EmailClient _client;
    private readonly string _fromAddress;

    public EmailCommunication(string connectionString = "endpoint=https://cs-c61afe16.europe.communication.azure.com/;accesskey=GA3Nh39Dle6feYTSVSa8sJn7eRaHyMh4DhNmzM0KQYevT9cCKO55JQQJ99AJACULyCpmwvJmAAAAAZCSmzrH", string fromAddress = "DoNotReply@309140df-25da-4123-9c4b-3bd60b413e89.azurecomm.net")
    {
        _client = new EmailClient(connectionString);
        _fromAddress = fromAddress;
    }

    public void Send(string toAddress, string subject, string body, string bodyPlainText)
    {
        EmailSendOperation emailSendOperation = _client.Send(
            WaitUntil.Completed,
            senderAddress: _fromAddress,
            recipientAddress: toAddress,
            subject: subject,
            htmlContent: body,
            plainTextContent: bodyPlainText);
    }
}
