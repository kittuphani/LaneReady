using LaneReady.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.RuleSets.Queries;

public sealed record GetRuleSetsQuery : IRequest<List<RuleSetSummaryDto>>;

public record RuleSetSummaryDto(
    Guid Id,
    string Version,
    bool IsDraft,
    bool IsApproved,
    bool IsActive,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string? AuthoredBy,
    string? ApprovedBy,
    DateTime? ApprovedAt);

public sealed class GetRuleSetsQueryHandler : IRequestHandler<GetRuleSetsQuery, List<RuleSetSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetRuleSetsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<List<RuleSetSummaryDto>> Handle(GetRuleSetsQuery request, CancellationToken cancellationToken) =>
        await _db.RuleSets
            .AsNoTracking()
            .OrderByDescending(r => r.EffectiveFrom)
            .Select(r => new RuleSetSummaryDto(
                r.Id, r.Version, r.IsDraft, r.IsApproved, r.IsActive,
                r.EffectiveFrom, r.EffectiveTo, r.AuthoredBy, r.ApprovedBy, r.ApprovedAt))
            .ToListAsync(cancellationToken);
}
