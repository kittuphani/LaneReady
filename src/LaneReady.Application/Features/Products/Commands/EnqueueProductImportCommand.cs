using FluentValidation;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Application.Common.QueueMessages;
using MediatR;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record EnqueueProductImportCommand(string BlobPath) : IRequest;

public sealed class EnqueueProductImportCommandValidator : AbstractValidator<EnqueueProductImportCommand>
{
    public EnqueueProductImportCommandValidator() =>
        RuleFor(x => x.BlobPath).NotEmpty();
}

public sealed class EnqueueProductImportCommandHandler : IRequestHandler<EnqueueProductImportCommand>
{
    private readonly IQueueService _queue;
    private readonly ICurrentUserService _currentUser;

    public EnqueueProductImportCommandHandler(IQueueService queue, ICurrentUserService currentUser)
    {
        _queue = queue;
        _currentUser = currentUser;
    }

    public async Task Handle(EnqueueProductImportCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new Common.Exceptions.ForbiddenException("No organisation associated.");

        var message = new ProductImportMessage(
            orgId,
            request.BlobPath,
            Guid.CreateVersion7(),
            "csv");

        await _queue.EnqueueAsync("product-imports", message, cancellationToken);
    }
}
