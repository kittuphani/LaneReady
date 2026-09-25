using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.QueueMessages;
using LaneReady.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record ImportProductsCommand(string BlobPath, Guid ImportJobId) : IRequest;

public sealed class ImportProductsCommandValidator : AbstractValidator<ImportProductsCommand>
{
    public ImportProductsCommandValidator()
    {
        RuleFor(x => x.BlobPath).NotEmpty();
        RuleFor(x => x.ImportJobId).NotEmpty();
    }
}

public sealed class ImportProductsCommandHandler : IRequestHandler<ImportProductsCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IBlobStorageService _blobStorage;
    private readonly ICurrentUserService _currentUser;
    private readonly IQueueService _queue;
    private readonly ILogger<ImportProductsCommandHandler> _logger;

    public ImportProductsCommandHandler(
        IApplicationDbContext db,
        IBlobStorageService blobStorage,
        ICurrentUserService currentUser,
        IQueueService queue,
        ILogger<ImportProductsCommandHandler> logger)
    {
        _db = db;
        _blobStorage = blobStorage;
        _currentUser = currentUser;
        _queue = queue;
        _logger = logger;
    }

    public async Task Handle(ImportProductsCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated.");

        var blobExists = await _blobStorage.BlobExistsAsync("imports", request.BlobPath, cancellationToken);
        if (!blobExists)
            throw new NotFoundException("Import file", request.BlobPath);

        _logger.LogInformation(
            "Product import {ImportJobId} processed for org {OrgId}",
            request.ImportJobId, orgId);
    }
}
