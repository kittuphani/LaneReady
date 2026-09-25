using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record ConfirmAiSuggestionCommand(Guid ProductId)
    : IRequest, IAuditableCommand
{
    public AuditAction AuditAction => AuditAction.Confirm;
    public string EntityType => "Product";
    public Guid EntityId => ProductId;
    public string? BeforeJson => null;
    public string? AfterJson => null;
}

public sealed class ConfirmAiSuggestionCommandValidator : AbstractValidator<ConfirmAiSuggestionCommand>
{
    public ConfirmAiSuggestionCommandValidator() =>
        RuleFor(x => x.ProductId).NotEmpty();
}

public sealed class ConfirmAiSuggestionCommandHandler : IRequestHandler<ConfirmAiSuggestionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ConfirmAiSuggestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ConfirmAiSuggestionCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.ProductId], cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        product.ConfirmAiSuggestion(_currentUser.UserId
            ?? throw new ForbiddenException("User identity required."));

        await _db.SaveChangesAsync(cancellationToken);
    }
}
