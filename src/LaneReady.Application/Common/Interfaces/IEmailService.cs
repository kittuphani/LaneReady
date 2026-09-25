namespace LaneReady.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);

    Task SendWithAttachmentAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string attachmentName,
        byte[] attachmentContent,
        string attachmentContentType,
        CancellationToken cancellationToken = default);
}
