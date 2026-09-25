using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;
using LaneReady.Domain.Events;
using LaneReady.Domain.Exceptions;

namespace LaneReady.Domain.Entities;

public sealed class Shipment : TenantedAuditableEntity
{
    private readonly List<ShipmentLine> _lines = [];
    private readonly List<EvidenceDocument> _evidence = [];

    public DateOnly ShipmentDate { get; private set; }
    public ShipmentRoute Route { get; private set; }
    public LaneDecision LaneDecision { get; private set; } = LaneDecision.NotDetermined;
    public string? LaneDecisionReason { get; private set; }
    public string? CarrierName { get; private set; }
    public ConsigneeType ConsigneeType { get; private set; }
    public ShipmentStatus Status { get; private set; } = ShipmentStatus.Draft;
    public string RuleSetVersion { get; private set; } = string.Empty;
    public Guid RuleSetId { get; private set; }
    public string? SourceOrderId { get; private set; }
    public string? Notes { get; private set; }
    public string? ApprovalOverrideReason { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public IReadOnlyCollection<ShipmentLine> Lines => _lines.AsReadOnly();
    public IReadOnlyCollection<EvidenceDocument> Evidence => _evidence.AsReadOnly();

    private Shipment() { }

    public static Shipment Create(
        Guid organisationId,
        DateOnly shipmentDate,
        ConsigneeType consigneeType,
        string ruleSetVersion,
        Guid ruleSetId,
        string? carrierName = null,
        string? sourceOrderId = null)
    {
        return new Shipment
        {
            OrganisationId = organisationId,
            ShipmentDate = shipmentDate,
            Route = ShipmentRoute.GbToNi,
            ConsigneeType = consigneeType,
            RuleSetVersion = ruleSetVersion,
            RuleSetId = ruleSetId,
            CarrierName = carrierName,
            SourceOrderId = sourceOrderId
        };
    }

    public void SetLaneDecision(LaneDecision decision, string? reason = null)
    {
        LaneDecision = decision;
        LaneDecisionReason = reason;
        AddDomainEvent(new ShipmentCreatedEvent(Id, OrganisationId, decision));
    }

    public void AddLine(ShipmentLine line) => _lines.Add(line);

    public void AttachEvidence(EvidenceDocument doc) => _evidence.Add(doc);

    public void Approve(string approvedByUserId, string? overrideReason = null)
    {
        if (LaneDecision == LaneDecision.Red && string.IsNullOrWhiteSpace(overrideReason))
            throw new DomainException("A reason is required to approve a red-lane shipment.");

        Status = ShipmentStatus.Approved;
        ApprovedBy = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        ApprovalOverrideReason = overrideReason;
    }

    public void MarkExported(string exportedBy)
    {
        if (Status != ShipmentStatus.Approved)
            throw new DomainException("Only approved shipments can be exported.");
        Status = ShipmentStatus.Exported;
    }

    public void Cancel() => Status = ShipmentStatus.Cancelled;

    public bool HasIncompleteLines =>
        _lines.Any(l => string.IsNullOrWhiteSpace(l.CommodityCode));
}
