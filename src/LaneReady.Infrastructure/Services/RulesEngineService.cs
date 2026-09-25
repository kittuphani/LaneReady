using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Enums;
using LaneReady.RulesEngine.Evaluation;
using LaneReady.RulesEngine.Models;
using LaneReady.RulesEngine.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Infrastructure.Services;

public class RulesEngineService : IRulesEngineService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRulesEvaluator _evaluator;

    public RulesEngineService(IApplicationDbContext dbContext, IRulesEvaluator evaluator)
    {
        _dbContext = dbContext;
        _evaluator = evaluator;
    }

    public async Task<LaneDecision> EvaluateLaneAsync(
        Guid organisationId,
        ConsigneeType consigneeType,
        decimal totalValueGbp,
        bool allProductsHaveCodes,
        CancellationToken cancellationToken = default)
    {
        var (definition, _) = await GetActiveDefinitionAsync(cancellationToken);

        var org = await _dbContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organisationId, cancellationToken);

        var context = new EvaluationContext
        {
            Organisation = new OrganisationContext
            {
                HasUkims = org?.HasValidUkims ?? false,
                UkimsExpired = org?.UkimsExpiry.HasValue == true
                    && org.UkimsExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow),
                PlanTier = org?.Plan.ToString() ?? "Trial",
                IsB2C = consigneeType == ConsigneeType.B2C
            },
            Shipment = new ShipmentContext
            {
                ConsigneeType = consigneeType.ToString(),
                TotalValueGbp = totalValueGbp,
                AllProductsHaveCodes = allProductsHaveCodes,
                LineCount = 0
            }
        };

        return _evaluator.EvaluateLane(context, definition);
    }

    public async Task<EligibilityStepResult> AnswerEligibilityQuestionAsync(
        string questionId,
        string answer,
        CancellationToken cancellationToken = default)
    {
        var (definition, _) = await GetActiveDefinitionAsync(cancellationToken);
        return _evaluator.AnswerQuestion(questionId, answer, definition);
    }

    public async Task<IReadOnlyList<EligibilityQuestion>> GetEligibilityQuestionsAsync(
        CancellationToken cancellationToken = default)
    {
        var (definition, _) = await GetActiveDefinitionAsync(cancellationToken);
        return definition.EligibilityQuestions.AsReadOnly();
    }

    public async Task<EligibilityQuestion?> GetFirstEligibilityQuestionAsync(
        CancellationToken cancellationToken = default)
    {
        var (definition, _) = await GetActiveDefinitionAsync(cancellationToken);
        return _evaluator.GetFirstQuestion(definition);
    }

    public async Task<(string Version, Guid Id)> GetActiveRuleSetInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var ruleSet = await _dbContext.RuleSets
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active rule set found. Please activate a rule set first.");

        return (ruleSet.Version, ruleSet.Id);
    }

    private async Task<(RuleSetDefinition Definition, Guid Id)> GetActiveDefinitionAsync(
        CancellationToken cancellationToken)
    {
        var ruleSet = await _dbContext.RuleSets
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active rule set found.");

        if (!RuleSetSerializer.TryDeserialize(ruleSet.RulesJson, out var definition, out var errors))
            throw new InvalidOperationException(
                $"Active rule set {ruleSet.Version} is invalid: {string.Join("; ", errors)}");

        return (definition!, ruleSet.Id);
    }
}
