using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Evidence.Commands;

public sealed record UploadEvidenceCommand(
    Guid ShipmentId,
    string FileName,
    string ContentType,
    Stream FileContent,
    EvidenceDocumentType DocumentType)
    : IRequest<Guid>, IAuditableCommand
{
    public AuditAction AuditAction => AuditAction.Create;
    public string EntityType => "EvidenceDocument";
    public Guid EntityId => ShipmentId;
    public string? BeforeJson => null;
    public string? AfterJson => null;
}

public sealed class UploadEvidenceCommandValidator : AbstractValidator<UploadEvidenceCommand>
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        { "application/pdf", "image/jpeg", "image/png", "text/csv", "text/plain" };

    public UploadEvidenceCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedTypes.Contains(ct))
            .WithMessage("File type not allowed. Permitted: PDF, JPEG, PNG, CSV");
    }
}

public sealed class UploadEvidenceCommandHandler : IRequestHandler<UploadEvidenceCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly IBlobStorageService _blobStorage;
    private readonly ICurrentUserService _currentUser;

    public UploadEvidenceCommandHandler(
        IApplicationDbContext db,
        IBlobStorageService blobStorage,
        ICurrentUserService currentUser)
    {
        _db = db;
        _blobStorage = blobStorage;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UploadEvidenceCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        // Verify shipment belongs to this org
        var shipment = await _db.Shipments.FindAsync([request.ShipmentId], cancellationToken)
            ?? throw new NotFoundException("Shipment", request.ShipmentId);

        var uploadResult = await _blobStorage.UploadEvidenceAsync(
            orgId, request.FileName, request.FileContent, request.ContentType, cancellationToken);

        var doc = EvidenceDocument.Create(
            orgId,
            request.DocumentType,
            request.FileName,
            uploadResult.ContainerName,
            uploadResult.BlobPath,
            uploadResult.Sha256Hash,
            uploadResult.FileSizeBytes,
            request.ContentType,
            request.ShipmentId);

        shipment.AttachEvidence(doc);
        _db.EvidenceDocuments.Add(doc);
        await _db.SaveChangesAsync(cancellationToken);

        return doc.Id;
    }
}
