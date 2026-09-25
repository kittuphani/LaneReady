using LaneReady.Application.Common.Interfaces;

namespace LaneReady.Infrastructure.Services;

// No-op stub implementations used in local development when Azure is not configured.

internal sealed class StubBlobStorageService : IBlobStorageService
{
    public Task<BlobUploadResult> UploadEvidenceAsync(
        Guid organisationId, string fileName, Stream content,
        string contentType, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlobUploadResult("dev-evidence", $"dev/{fileName}", "dev-hash-00", 0));

    public Task<Uri> GenerateDownloadSasUriAsync(
        string blobContainerName, string blobPath,
        TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.FromResult(new Uri($"https://localhost/dev-blob/{blobPath}"));

    public Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
        => Task.FromResult("dev-sha256");

    public Task<bool> BlobExistsAsync(string containerName, string blobPath,
        CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<BlobUploadResult> UploadImportFileAsync(
        Stream content, string fileName, CancellationToken cancellationToken = default)
        => Task.FromResult(new BlobUploadResult("dev-imports", $"dev/{fileName}", "dev-hash-00", 0));
}

internal sealed class StubAiService : IAiService
{
    public Task<AiProductSuggestion> SuggestProductDataAsync(
        string sku, string currentDescription, string countryOfOrigin,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new AiProductSuggestion(
            currentDescription,
            "61091000",
            0.85m,
            "Dev stub — Azure OpenAI not configured",
            "stub-v0"));
}

internal sealed class StubEmailService : IEmailService
{
    public Task SendAsync(string toEmail, string toName, string subject, string htmlBody,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEV EMAIL] To: {toEmail} ({toName}) | Subject: {subject}");
        return Task.CompletedTask;
    }

    public Task SendWithAttachmentAsync(string toEmail, string toName, string subject, string htmlBody,
        string attachmentName, byte[] attachmentContent, string attachmentContentType,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[DEV EMAIL+ATTACH] To: {toEmail} | Subject: {subject} | Attachment: {attachmentName}");
        return Task.CompletedTask;
    }
}

internal sealed class StubQueueService : IQueueService
{
    public Task EnqueueAsync<T>(string queueName, T message,
        CancellationToken cancellationToken = default) where T : class
    {
        Console.WriteLine($"[DEV QUEUE] {queueName}: {System.Text.Json.JsonSerializer.Serialize(message)}");
        return Task.CompletedTask;
    }
}

internal sealed class StubStripeService : IStripeService
{
    public Task<string> CreateCheckoutSessionAsync(
        string organisationId, LaneReady.Domain.Enums.PlanTier plan,
        string successUrl, string cancelUrl,
        CancellationToken cancellationToken = default)
        => Task.FromResult(successUrl + "&dev_stripe=true");

    public Task<string> CreateCustomerPortalSessionAsync(
        string stripeCustomerId, string returnUrl,
        CancellationToken cancellationToken = default)
        => Task.FromResult(returnUrl + "&dev_portal=true");
}
