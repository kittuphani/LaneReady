using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Features.Organisations.Queries;

public sealed record GetOrganisationQuery : IRequest<OrganisationDto>;

public record OrganisationDto(
    Guid Id,
    string Name,
    string GbEori,
    string? XiEori,
    string? Vat,
    string? UkimsNumber,
    DateOnly? UkimsExpiry,
    bool HasValidUkims,
    PlanTier Plan,
    bool IsTrialActive,
    DateTime? TrialEndsAt);

public sealed class GetOrganisationQueryHandler
    : IRequestHandler<GetOrganisationQuery, OrganisationDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetOrganisationQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<OrganisationDto> Handle(
        GetOrganisationQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganisationId
            ?? throw new ForbiddenException("No organisation associated with this user.");

        var org = await _db.Organisations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.Id == orgId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Organisation), orgId);

        return new OrganisationDto(
            org.Id,
            org.Name,
            org.GbEori.Value,
            org.XiEori?.Value,
            org.Vat?.Value,
            org.UkimsNumber,
            org.UkimsExpiry,
            org.HasValidUkims,
            org.Plan,
            org.IsTrialActive,
            org.TrialEndsAt);
    }
}
