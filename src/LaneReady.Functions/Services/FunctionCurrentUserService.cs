using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;

namespace LaneReady.Functions.Services;

public class FunctionCurrentUserService : ICurrentUserService, IFunctionCurrentUserService
{
    private static readonly AsyncLocal<Guid?> _organisationId = new();

    public string? UserId => "function-worker";
    public string? UserEmail => null;
    public string? UserFullName => null;
    public Guid? LaneReadyUserId => null;
    public Guid? OrganisationId => _organisationId.Value;
    public UserRole? UserRole => Domain.Enums.UserRole.Admin;
    public bool HasOrganisation => _organisationId.Value.HasValue;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;

    public bool IsInRole(UserRole minimumRole) => true;

    public void SetOrganisationId(Guid organisationId) =>
        _organisationId.Value = organisationId;
}
