using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.RuleSets.Commands;

public sealed record ActivateRuleSetCommand(Guid RuleSetId, DateOnly EffectiveFrom) : IRequest;

public sealed class ActivateRuleSetCommandValidator : AbstractValidator<ActivateRuleSetCommand>
{
    public ActivateRuleSetCommandValidator() =>
        RuleFor(x => x.RuleSetId).NotEmpty();
}

public sealed class ActivateRuleSetCommandHandler : IRequestHandler<ActivateRuleSetCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ActivateRuleSetCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ActivateRuleSetCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(UserRole.Admin))
            throw new ForbiddenException("Only admins can activate rule sets.");

        // Retire the current active rule set first
        var currentActive = await _db.RuleSets
            .FirstOrDefaultAsync(r => r.IsActive, cancellationToken);
        currentActive?.Retire(request.EffectiveFrom.AddDays(-1));

        var ruleSet = await _db.RuleSets.FindAsync([request.RuleSetId], cancellationToken)
            ?? throw new NotFoundException("RuleSet", request.RuleSetId);

        ruleSet.Activate(request.EffectiveFrom);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
