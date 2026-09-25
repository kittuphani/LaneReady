namespace LaneReady.Application.Common.Interfaces;

public interface IAiService
{
    Task<AiProductSuggestion> SuggestProductDataAsync(
        string sku,
        string currentDescription,
        string countryOfOrigin,
        CancellationToken cancellationToken = default);
}

public record AiProductSuggestion(
    string SuggestedDescription,
    string SuggestedCommodityCode,
    decimal ConfidenceScore,
    string Reasoning,
    string ModelVersion);
