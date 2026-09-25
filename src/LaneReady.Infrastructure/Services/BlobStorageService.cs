using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using LaneReady.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LaneReady.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private const string EvidenceContainer = "evidence";
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<BlobUploadResult> UploadEvidenceAsync(
        Guid organisationId,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var blobPath = $"{organisationId}/{Guid.CreateVersion7()}/{fileName}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(EvidenceContainer);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobPath);

        string sha256Hash;
        long fileSize;
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        fileSize = buffer.Length;
        var hashBytes = SHA256.HashData(buffer.ToArray());
        sha256Hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        buffer.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(buffer, headers, cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Uploaded evidence blob {BlobPath} ({ContentType}, sha256={Hash})",
            blobPath, contentType, sha256Hash);

        return new BlobUploadResult(EvidenceContainer, blobPath, sha256Hash, fileSize);
    }

    public async Task<Uri> GenerateDownloadSasUriAsync(
        string blobContainerName,
        string blobPath,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobContainerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder);
    }

    public async Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<BlobUploadResult> UploadImportFileAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        const string importContainer = "imports";
        var blobPath = $"{Guid.CreateVersion7()}/{fileName}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(importContainer);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobPath);

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var hashBytes = SHA256.HashData(buffer.ToArray());
        var sha256Hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        buffer.Position = 0;

        await blobClient.UploadAsync(buffer, new BlobHttpHeaders { ContentType = "text/csv" },
            cancellationToken: cancellationToken);

        return new BlobUploadResult(importContainer, blobPath, sha256Hash, buffer.Length);
    }

    public async Task<bool> BlobExistsAsync(
        string containerName,
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }
}
