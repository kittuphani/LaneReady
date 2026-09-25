using LaneReady.Domain.Common;
using LaneReady.Domain.Exceptions;

namespace LaneReady.Domain.Entities;

public sealed class RuleSet : AuditableEntity
{
    public string Version { get; private set; } = default!;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string RulesJson { get; private set; } = default!;
    public bool IsDraft { get; private set; } = true;
    public bool IsApproved { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public string? AuthoredBy { get; private set; }

    private RuleSet() { }

    public static RuleSet CreateDraft(string version, string rulesJson, string authoredByUserId)
    {
        return new RuleSet
        {
            Version = version,
            RulesJson = rulesJson,
            IsDraft = true,
            IsApproved = false,
            IsActive = false,
            AuthoredBy = authoredByUserId
        };
    }

    public void UpdateRulesJson(string rulesJson)
    {
        if (!IsDraft)
            throw new DomainException("Published rule sets cannot be edited. Create a new draft.");
        RulesJson = rulesJson;
    }

    public void Approve(string approvedByUserId)
    {
        if (approvedByUserId == AuthoredBy)
            throw new DomainException("The author cannot approve their own rule set changes (four-eyes principle).");
        if (!IsDraft)
            throw new DomainException("Only draft rule sets can be approved.");

        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedByUserId;
    }

    public void Activate(DateOnly effectiveFrom)
    {
        if (!IsApproved)
            throw new DomainException("Rule set must be approved before activation.");

        IsDraft = false;
        IsActive = true;
        EffectiveFrom = effectiveFrom;
    }

    public void Retire(DateOnly effectiveTo)
    {
        IsActive = false;
        EffectiveTo = effectiveTo;
    }
}
