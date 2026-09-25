using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.RuleSets.Commands;

public sealed record ApproveRuleSetCommand(Guid RuleSetId) : IRequest;

public sealed class ApproveRuleSetCommandValidator : AbstractValidator<ApproveRuleSetCommand>
{
    public ApproveRuleSetCommandValidator() =>
        RuleFor(x => x.RuleSetId).NotEmpty();
}

public sealed class ApproveRuleSetCommandHandler : IRequestHandler<ApproveRuleSetCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApproveRuleSetCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ApproveRuleSetCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(UserRole.Admin))
            throw new ForbiddenException("Only admins can approve rule sets.");

        var ruleSet = await _db.RuleSets.FindAsync([request.RuleSetId], cancellationToken)
            ?? throw new NotFoundException("RuleSet", request.RuleSetId);

        ruleSet.Approve(_currentUser.UserId ?? throw new ForbiddenException("User identity required."));
        await _db.SaveChangesAsync(cancellationToken);
    }
}
