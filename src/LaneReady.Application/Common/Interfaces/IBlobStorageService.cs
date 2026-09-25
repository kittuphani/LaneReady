namespace LaneReady.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<BlobUploadResult> UploadEvidenceAsync(
        Guid organisationId,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Uri> GenerateDownloadSasUriAsync(
        string blobContainerName,
        string blobPath,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default);

    Task<bool> BlobExistsAsync(
        string containerName,
        string blobPath,
        CancellationToken cancellationToken = default);

    Task<BlobUploadResult> UploadImportFileAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default);
}

public record BlobUploadResult(
    string ContainerName,
    string BlobPath,
    string Sha256Hash,
    long FileSizeBytes);
