using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;
using LaneReady.Domain.Events;
using LaneReady.Domain.ValueObjects;

namespace LaneReady.Domain.Entities;

public sealed class Organisation : AuditableEntity
{
    private readonly List<User> _users = [];

    public string Name { get; private set; } = default!;
    public EoriNumber GbEori { get; private set; } = default!;
    public EoriNumber? XiEori { get; private set; }
    public VatNumber? Vat { get; private set; }
    public string? UkimsNumber { get; private set; }
    public DateOnly? UkimsExpiry { get; private set; }
    public PlanTier Plan { get; private set; } = PlanTier.Trial;
    public DateTime? TrialEndsAt { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Organisation() { }

    public static Organisation Create(string name, EoriNumber gbEori, EoriNumber? xiEori = null, VatNumber? vat = null)
    {
        var org = new Organisation
        {
            Name = name,
            GbEori = gbEori,
            XiEori = xiEori,
            Vat = vat,
            Plan = PlanTier.Trial,
            TrialEndsAt = DateTime.UtcNow.AddDays(14)
        };
        org.AddDomainEvent(new OrganisationCreatedEvent(org.Id, name));
        return org;
    }

    public void RecordUkims(string number, DateOnly expiry)
    {
        UkimsNumber = number;
        UkimsExpiry = expiry;
    }

    public void UpdateXiEori(EoriNumber xiEori) => XiEori = xiEori;

    public void ChangePlan(PlanTier plan) => Plan = plan;

    public void UpdateStripeIds(string customerId, string subscriptionId)
    {
        StripeCustomerId = customerId;
        StripeSubscriptionId = subscriptionId;
    }

    public bool IsTrialActive => Plan == PlanTier.Trial && TrialEndsAt.HasValue && TrialEndsAt > DateTime.UtcNow;

    public bool HasValidUkims =>
        UkimsNumber is not null &&
        UkimsExpiry.HasValue &&
        UkimsExpiry.Value >= DateOnly.FromDateTime(DateTime.UtcNow);
}
