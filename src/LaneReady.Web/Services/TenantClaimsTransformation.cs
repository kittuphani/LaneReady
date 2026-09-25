using System.Security.Claims;
using LaneReady.Domain.Entities;
using LaneReady.Domain.Enums;
using LaneReady.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Web.Services;

/// <summary>
/// Runs after Entra External ID authenticates the user.
/// Upserts the LaneReady User record and adds lr_user_id / org_id / user_role claims.
/// Uses IServiceProvider.CreateScope so it can resolve scoped DbContext safely.
/// Uses IgnoreQueryFilters — tenant context is not yet established at this point.
/// </summary>
public class TenantClaimsTransformation : IClaimsTransformation
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantClaimsTransformation> _logger;

    public TenantClaimsTransformation(
        IServiceProvider serviceProvider,
        ILogger<TenantClaimsTransformation> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var entraObjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrEmpty(entraObjectId))
            return principal;

        // Already transformed this request
        if (principal.HasClaim(c => c.Type == "lr_user_id"))
            return principal;

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // IgnoreQueryFilters: tenant not yet known; we're locating the user record
            var user = await dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

            if (user is null)
            {
                // First sign-in — create unlinked user
                var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email") ?? string.Empty;
                var fullName = principal.FindFirstValue(ClaimTypes.Name)
                    ?? principal.FindFirstValue("name") ?? email;

                user = User.CreateUnlinked(entraObjectId, email, fullName);
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Created new user {UserId} for Entra OID {EntraOid}", user.Id, entraObjectId);
            }
            else
            {
                user.RecordLogin();
                await dbContext.SaveChangesAsync();
            }

            var identity = (ClaimsIdentity)principal.Identity!;
            identity.AddClaim(new Claim("lr_user_id", user.Id.ToString()));

            if (user.HasOrganisation)
            {
                identity.AddClaim(new Claim("org_id", user.OrganisationId.ToString()));
                identity.AddClaim(new Claim("user_role", user.Role.ToString()));
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantClaimsTransformation failed for Entra OID {EntraOid}", entraObjectId);
            // Return principal without extra claims — auth will succeed but app will redirect to onboarding
            return principal;
        }
    }
}
