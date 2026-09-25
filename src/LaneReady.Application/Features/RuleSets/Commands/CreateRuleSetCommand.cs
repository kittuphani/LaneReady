using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using LaneReady.Domain.Enums;
using LaneReady.RulesEngine.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.RuleSets.Commands;

public sealed record CreateRuleSetCommand(
    string Version,
    string RulesJson) : IRequest<Guid>;

public sealed class CreateRuleSetCommandValidator : AbstractValidator<CreateRuleSetCommand>
{
    public CreateRuleSetCommandValidator()
    {
        RuleFor(x => x.Version).NotEmpty().MaximumLength(50)
            .Matches(@"^\d{4}-\d{2}[a-z]?$").WithMessage("Version must be in format YYYY-MM or YYYY-MMa");
        RuleFor(x => x.RulesJson).NotEmpty()
            .Must(json => RuleSetSerializer.TryDeserialize(json, out _, out _))
            .WithMessage("RulesJson is not valid — check JSON schema");
    }
}

public sealed class CreateRuleSetCommandHandler : IRequestHandler<CreateRuleSetCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateRuleSetCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateRuleSetCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole(UserRole.Admin))
            throw new ForbiddenException("Only admins can create rule sets.");

        var exists = await _db.RuleSets
            .AnyAsync(r => r.Version == request.Version, cancellationToken);
        if (exists)
            throw new ConflictException($"A rule set with version '{request.Version}' already exists.");

        var ruleSet = RuleSet.CreateDraft(
            request.Version,
            request.RulesJson,
            _currentUser.UserId ?? "system");

        _db.RuleSets.Add(ruleSet);
        await _db.SaveChangesAsync(cancellationToken);
        return ruleSet.Id;
    }
}
