using System.Security.Claims;
using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace LaneReady.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? UserEmail => User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? UserFullName => User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("name");

    public Guid? LaneReadyUserId
    {
        get
        {
            var claim = User?.FindFirstValue("lr_user_id");
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? OrganisationId
    {
        get
        {
            var claim = User?.FindFirstValue("org_id");
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public UserRole? UserRole
    {
        get
        {
            var claim = User?.FindFirstValue("user_role");
            return Enum.TryParse<UserRole>(claim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool HasOrganisation => OrganisationId.HasValue;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsInRole(UserRole minimumRole)
    {
        if (UserRole is null) return false;
        if (UserRole == Domain.Enums.UserRole.Admin) return true;
        return (int)UserRole.Value <= (int)minimumRole;
    }
}
