using LaneReady.Domain.Interfaces;

namespace LaneReady.Domain.Common;

public abstract class TenantedAuditableEntity : AuditableEntity, IHasTenantId
{
    public Guid OrganisationId { get; set; }
}
