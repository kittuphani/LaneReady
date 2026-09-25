using LaneReady.Domain.ValueObjects;

namespace LaneReady.Domain.Entities;

public class ProductAiSuggestion
{
    public CommodityCode? SuggestedCommodityCode { get; set; }
    public string? SuggestedDescription { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string ModelVersion { get; set; } = string.Empty;

    public bool IsHighConfidence => ConfidenceScore >= 0.8m;
    public bool IsMediumConfidence => ConfidenceScore is >= 0.5m and < 0.8m;
    public bool IsLowConfidence => ConfidenceScore < 0.5m;

    public string ConfidenceLabel => ConfidenceScore switch
    {
        >= 0.8m => "High",
        >= 0.5m => "Medium",
        _ => "Low"
    };
}
