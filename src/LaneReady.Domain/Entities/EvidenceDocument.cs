using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;
using LaneReady.Domain.Events;

namespace LaneReady.Domain.Entities;

public sealed class EvidenceDocument : TenantedAuditableEntity
{
    public EvidenceDocumentType DocumentType { get; private set; }
    public string FileName { get; private set; } = default!;
    public string BlobContainerName { get; private set; } = default!;
    public string BlobPath { get; private set; } = default!;
    public string Sha256Hash { get; private set; } = default!;
    public long FileSizeBytes { get; private set; }
    public string ContentType { get; private set; } = default!;
    public DateOnly RetainUntil { get; private set; }
    public bool IsImmutable { get; private set; }
    public Guid? ShipmentId { get; private set; }

    private EvidenceDocument() { }

    public static EvidenceDocument Create(
        Guid organisationId,
        EvidenceDocumentType documentType,
        string fileName,
        string blobContainerName,
        string blobPath,
        string sha256Hash,
        long fileSizeBytes,
        string contentType,
        Guid? shipmentId = null)
    {
        var doc = new EvidenceDocument
        {
            OrganisationId = organisationId,
            DocumentType = documentType,
            FileName = fileName,
            BlobContainerName = blobContainerName,
            BlobPath = blobPath,
            Sha256Hash = sha256Hash,
            FileSizeBytes = fileSizeBytes,
            ContentType = contentType,
            RetainUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)),
            IsImmutable = true,
            ShipmentId = shipmentId
        };
        doc.AddDomainEvent(new EvidenceUploadedEvent(doc.Id, organisationId, shipmentId));
        return doc;
    }

    public bool IsRetentionPeriodActive =>
        RetainUntil >= DateOnly.FromDateTime(DateTime.UtcNow);
}
