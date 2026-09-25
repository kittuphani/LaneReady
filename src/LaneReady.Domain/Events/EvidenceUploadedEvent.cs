using LaneReady.Domain.Common;

namespace LaneReady.Domain.Events;

public sealed record EvidenceUploadedEvent(
    Guid DocumentId,
    Guid OrganisationId,
    Guid? ShipmentId) : DomainEvent;
