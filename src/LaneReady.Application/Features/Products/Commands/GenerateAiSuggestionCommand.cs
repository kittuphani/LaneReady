using FluentValidation;
using LaneReady.Application.Common.Exceptions;
using LaneReady.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LaneReady.Application.Features.Products.Commands;

public sealed record GenerateAiSuggestionCommand(Guid ProductId) : IRequest;

public sealed class GenerateAiSuggestionCommandValidator : AbstractValidator<GenerateAiSuggestionCommand>
{
    public GenerateAiSuggestionCommandValidator() =>
        RuleFor(x => x.ProductId).NotEmpty();
}

public sealed class GenerateAiSuggestionCommandHandler : IRequestHandler<GenerateAiSuggestionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IAiService _ai;
    private readonly ILogger<GenerateAiSuggestionCommandHandler> _logger;

    public GenerateAiSuggestionCommandHandler(
        IApplicationDbContext db,
        IAiService ai,
        ILogger<GenerateAiSuggestionCommandHandler> logger)
    {
        _db = db;
        _ai = ai;
        _logger = logger;
    }

    public async Task Handle(GenerateAiSuggestionCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync([request.ProductId], cancellationToken)
            ?? throw new NotFoundException("Product", request.ProductId);

        var suggestion = await _ai.SuggestProductDataAsync(
            product.Sku, product.Description ?? product.Title, product.CountryOfOrigin ?? "",
            cancellationToken);

        var aiSuggestion = new Domain.Entities.ProductAiSuggestion
        {
            SuggestedCommodityCode = suggestion.SuggestedCommodityCode is not null
                ? new Domain.ValueObjects.CommodityCode(suggestion.SuggestedCommodityCode)
                : null,
            SuggestedDescription = suggestion.SuggestedDescription,
            ConfidenceScore = suggestion.ConfidenceScore,
            Reasoning = suggestion.Reasoning,
            ModelVersion = suggestion.ModelVersion,
            GeneratedAt = DateTime.UtcNow
        };

        product.ApplyAiSuggestion(aiSuggestion);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AI suggestion generated for product {ProductId} with confidence {Confidence}",
            product.Id, suggestion.ConfidenceScore);
    }
}
