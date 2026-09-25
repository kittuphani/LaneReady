using LaneReady.Domain.Common;

namespace LaneReady.Domain.Events;

public sealed record OrganisationCreatedEvent(Guid OrganisationId, string Name) : DomainEvent;
