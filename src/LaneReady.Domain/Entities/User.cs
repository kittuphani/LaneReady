using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;

namespace LaneReady.Domain.Entities;

public sealed class User : TenantedAuditableEntity
{
    public string EntraObjectId { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public static User CreateUnlinked(string entraObjectId, string email, string fullName)
    {
        return new User
        {
            EntraObjectId = entraObjectId,
            Email = email,
            FullName = fullName,
            Role = UserRole.Owner,
            OrganisationId = Guid.Empty
        };
    }

    public void LinkToOrganisation(Guid organisationId, UserRole role)
    {
        OrganisationId = organisationId;
        Role = role;
    }

    public void ChangeRole(UserRole role) => Role = role;

    public void Deactivate() => IsActive = false;

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    public bool HasOrganisation => OrganisationId != Guid.Empty;
}
