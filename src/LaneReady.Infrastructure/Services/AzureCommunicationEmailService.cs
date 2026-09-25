using Azure.Communication.Email;
using LaneReady.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LaneReady.Infrastructure.Services;

public class AzureCommunicationEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderAddress;
    private readonly ILogger<AzureCommunicationEmailService> _logger;

    public AzureCommunicationEmailService(
        EmailClient emailClient,
        IConfiguration configuration,
        ILogger<AzureCommunicationEmailService> logger)
    {
        _emailClient = emailClient;
        _senderAddress = configuration["Email:SenderAddress"]
            ?? throw new InvalidOperationException("Email:SenderAddress is not configured");
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage(
            senderAddress: _senderAddress,
            content: new EmailContent(subject) { Html = htmlBody },
            recipients: new EmailRecipients([new EmailAddress(toEmail, toName)]));

        var operation = await _emailClient.SendAsync(
            Azure.WaitUntil.Started,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Email queued to {ToEmail}, operation ID: {OperationId}",
            toEmail, operation.Id);
    }

    public async Task SendWithAttachmentAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string attachmentName,
        byte[] attachmentContent,
        string attachmentContentType,
        CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage(
            senderAddress: _senderAddress,
            content: new EmailContent(subject) { Html = htmlBody },
            recipients: new EmailRecipients([new EmailAddress(toEmail, toName)]));

        message.Attachments.Add(new EmailAttachment(
            attachmentName,
            attachmentContentType,
            new BinaryData(attachmentContent)));

        var operation = await _emailClient.SendAsync(
            Azure.WaitUntil.Started,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Email with attachment queued to {ToEmail}, operation ID: {OperationId}",
            toEmail, operation.Id);
    }
}
