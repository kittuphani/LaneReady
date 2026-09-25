namespace LaneReady.Domain.Interfaces;

public interface IHasTenantId
{
    Guid OrganisationId { get; }
}
