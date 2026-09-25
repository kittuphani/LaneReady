using System.Text.Json;
using Azure.AI.OpenAI;
using LaneReady.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace LaneReady.Infrastructure.Services;

public class AzureOpenAiService : IAiService
{
    private readonly AzureOpenAIClient _client;
    private readonly ILogger<AzureOpenAiService> _logger;
    private const string DeploymentName = "gpt-4o";
    private const string ModelVersion = "gpt-4o-2024-11-20";
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AzureOpenAiService(AzureOpenAIClient client, ILogger<AzureOpenAiService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<AiProductSuggestion> SuggestProductDataAsync(
        string sku,
        string currentDescription,
        string countryOfOrigin,
        CancellationToken cancellationToken = default)
    {
        // Only product data — no PII, no org data, no financial data in this prompt
        var prompt = $$"""
            You are a UK customs classification expert. Suggest the most appropriate 8-digit UK Trade Tariff commodity code.

            Product SKU: {{sku}}
            Product description: {{currentDescription}}
            Country of origin: {{countryOfOrigin}}

            Respond with valid JSON only in this exact format:
            {
              "suggestedCommodityCode": "12345678",
              "suggestedDescription": "Clear customs-appropriate description",
              "confidenceScore": 0.85,
              "reasoning": "Brief explanation of why this code applies"
            }

            If you cannot determine a code with confidence above 0.5, still return a best guess with a low confidenceScore.
            """;

        try
        {
            var chatClient = _client.GetChatClient(DeploymentName);
            var response = await chatClient.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                new ChatCompletionOptions { MaxOutputTokenCount = 500, Temperature = 0f },
                cancellationToken);

            var json = response.Value.Content[0].Text;

            // Strip markdown code blocks if model wrapped the JSON
            if (json.Contains("```"))
            {
                var start = json.IndexOf('{');
                var end = json.LastIndexOf('}');
                if (start >= 0 && end > start)
                    json = json[start..(end + 1)];
            }

            var result = JsonSerializer.Deserialize<AiResponseDto>(json, _jsonOptions);

            if (result is null)
                throw new InvalidOperationException("AI returned empty response");

            return new AiProductSuggestion(
                result.SuggestedDescription,
                result.SuggestedCommodityCode,
                (decimal)result.ConfidenceScore,
                result.Reasoning,
                ModelVersion);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "AI suggestion failed for SKU {Sku}", sku);
            throw;
        }
    }

    private sealed record AiResponseDto(
        string SuggestedCommodityCode,
        string SuggestedDescription,
        double ConfidenceScore,
        string Reasoning);
}
