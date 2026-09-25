using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;

namespace LaneReady.Application.Features.Ukims.Commands;

public sealed record RecordUkimsCommand(
    string UkimsNumber,
    DateOnly ExpiryDate)
    : IRequest, IAuditableCommand
{
    public AuditAction AuditAction => AuditAction.Update;
    public string EntityType => "Organisation";
    public Guid EntityId => Guid.Empty; // replaced at handler time
    public string? BeforeJson => null;
    public string? AfterJson => null;
}

public sealed class RecordUkimsCommandValidator : AbstractValidator<RecordUkimsCommand>
{
    public RecordUkimsCommandValidator()
    {
        RuleFor(x => x.UkimsNumber).NotEmpty().MaximumLength(50)
            .Matches(@"^UKIMS\d+$").WithMessage("UKIMS number must start with 'UKIMS' followed by digits");
        RuleFor(x => x.ExpiryDate).GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Expiry date must be in the future");
    }
}

public sealed class RecordUkimsCommandHandler : IRequestHandler<RecordUkimsCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RecordUkimsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(RecordUkimsCommand request, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        if (!_currentUser.IsInRole(UserRole.Owner))
            throw new ForbiddenException("Only owners can record UKIMS authorisation.");

        var org = await _db.Organisations.FindAsync([orgId], cancellationToken)
            ?? throw new NotFoundException("Organisation", orgId);

        org.RecordUkims(request.UkimsNumber, request.ExpiryDate);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
