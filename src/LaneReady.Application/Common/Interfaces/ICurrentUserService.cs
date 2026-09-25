using LaneReady.Domain.Enums;

namespace LaneReady.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserEmail { get; }
    string? UserFullName { get; }
    Guid? LaneReadyUserId { get; }
    Guid? OrganisationId { get; }
    UserRole? UserRole { get; }
    bool IsAuthenticated { get; }
    bool HasOrganisation { get; }
    string? IpAddress { get; }
    bool IsInRole(UserRole minimumRole);
}
