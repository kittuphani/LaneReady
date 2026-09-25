using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Entities;
using MediatR;

namespace LaneReady.Application.Common.Behaviours;

public sealed class AuditBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AuditBehaviour(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        if (request is IAuditableCommand auditable && _currentUser.IsAuthenticated)
        {
            var log = AuditLog.Create(
                _currentUser.OrganisationId ?? Guid.Empty,
                _currentUser.UserId ?? "unknown",
                _currentUser.UserEmail ?? string.Empty,
                auditable.AuditAction,
                auditable.EntityType,
                auditable.EntityId,
                auditable.BeforeJson,
                auditable.AfterJson,
                _currentUser.IpAddress);

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
