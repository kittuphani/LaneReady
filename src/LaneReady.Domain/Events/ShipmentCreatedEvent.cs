using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;

namespace LaneReady.Domain.Events;

public sealed record ShipmentCreatedEvent(
    Guid ShipmentId,
    Guid OrganisationId,
    LaneDecision LaneDecision) : DomainEvent;
