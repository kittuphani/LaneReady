using LaneReady.Domain.Common;
using LaneReady.Domain.Enums;
using LaneReady.Domain.Events;
using LaneReady.Domain.Exceptions;
using LaneReady.Domain.ValueObjects;

namespace LaneReady.Domain.Entities;

public sealed class Product : TenantedAuditableEntity
{
    public string Sku { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public CommodityCode? ConfirmedCommodityCode { get; private set; }
    public decimal GrossWeightKg { get; private set; }
    public decimal NetWeightKg { get; private set; }
    public Money UnitValue { get; private set; } = default!;
    public string CountryOfOrigin { get; private set; } = string.Empty;
    public bool NirmsFlag { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.NeedsReview;
    public string? SourceStoreId { get; private set; }
    public string? ParentProductId { get; private set; }

    public ProductAiSuggestion? AiSuggestion { get; private set; }
    public bool AiSuggestionConfirmed { get; private set; }
    public DateTime? AiSuggestionConfirmedAt { get; private set; }
    public string? AiSuggestionConfirmedBy { get; private set; }

    private Product() { }

    public static Product Create(
        Guid organisationId,
        string sku,
        string title,
        string description,
        decimal grossWeightKg,
        decimal netWeightKg,
        Money unitValue,
        string countryOfOrigin)
    {
        return new Product
        {
            OrganisationId = organisationId,
            Sku = sku,
            Title = title,
            Description = description,
            GrossWeightKg = grossWeightKg,
            NetWeightKg = netWeightKg,
            UnitValue = unitValue,
            CountryOfOrigin = countryOfOrigin,
            Status = DetermineInitialStatus(sku, description, grossWeightKg, unitValue)
        };
    }

    public void UpdateDetails(
        string title,
        string description,
        decimal grossWeightKg,
        decimal netWeightKg,
        Money unitValue,
        string countryOfOrigin)
    {
        Title = title;
        Description = description;
        GrossWeightKg = grossWeightKg;
        NetWeightKg = netWeightKg;
        UnitValue = unitValue;
        CountryOfOrigin = countryOfOrigin;
        RecalculateStatus();
    }

    public void ApplyAiSuggestion(ProductAiSuggestion suggestion)
    {
        AiSuggestion = suggestion;
        AiSuggestionConfirmed = false;
    }

    public void ConfirmAiSuggestion(string confirmedByUserId)
    {
        if (AiSuggestion is null)
            throw new DomainException("No AI suggestion to confirm.");

        if (AiSuggestion.SuggestedCommodityCode is not null)
            ConfirmedCommodityCode = AiSuggestion.SuggestedCommodityCode;

        if (!string.IsNullOrWhiteSpace(AiSuggestion.SuggestedDescription))
            Description = AiSuggestion.SuggestedDescription;

        AiSuggestionConfirmed = true;
        AiSuggestionConfirmedAt = DateTime.UtcNow;
        AiSuggestionConfirmedBy = confirmedByUserId;

        RecalculateStatus();
        AddDomainEvent(new ProductConfirmedEvent(Id, OrganisationId, ConfirmedCommodityCode));
    }

    public void OverrideCommodityCode(CommodityCode code, string overriddenByUserId)
    {
        ConfirmedCommodityCode = code;
        AiSuggestionConfirmedBy = overriddenByUserId;
        AiSuggestionConfirmedAt = DateTime.UtcNow;
        RecalculateStatus();
        AddDomainEvent(new ProductConfirmedEvent(Id, OrganisationId, code));
    }

    public void SetNirmsFlag(bool nirmsRequired) => NirmsFlag = nirmsRequired;

    public void SetSourceStore(string storeId, string? parentId = null)
    {
        SourceStoreId = storeId;
        ParentProductId = parentId;
    }

    private static ProductStatus DetermineInitialStatus(
        string sku, string description, decimal weightKg, Money value)
    {
        if (string.IsNullOrWhiteSpace(sku) || weightKg <= 0 || value.Amount <= 0)
            return ProductStatus.MissingData;
        if (IsVagueDescription(description))
            return ProductStatus.NeedsReview;
        return ProductStatus.NeedsReview;
    }

    private void RecalculateStatus()
    {
        if (GrossWeightKg <= 0 || UnitValue.Amount <= 0)
        {
            Status = ProductStatus.MissingData;
            return;
        }
        if (ConfirmedCommodityCode is not null && !IsVagueDescription(Description))
            Status = ProductStatus.Complete;
        else
            Status = ProductStatus.NeedsReview;
    }

    private static readonly HashSet<string> VagueTerms = new(StringComparer.OrdinalIgnoreCase)
    {
        "clothing", "gift", "gifts", "parts", "samples", "sample", "goods",
        "merchandise", "product", "item", "items", "accessories", "other"
    };

    private static bool IsVagueDescription(string description) =>
        string.IsNullOrWhiteSpace(description) ||
        description.Length < 10 ||
        VagueTerms.Contains(description.Trim());
}
