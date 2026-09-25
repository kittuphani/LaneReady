using LaneReady.Domain.Common;
using LaneReady.Domain.ValueObjects;

namespace LaneReady.Domain.Events;

public sealed record ProductConfirmedEvent(
    Guid ProductId,
    Guid OrganisationId,
    CommodityCode? ConfirmedCode) : DomainEvent;
