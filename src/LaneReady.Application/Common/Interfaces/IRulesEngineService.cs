using LaneReady.Domain.Enums;
using LaneReady.RulesEngine.Evaluation;
using LaneReady.RulesEngine.Models;

namespace LaneReady.Application.Common.Interfaces;

public interface IRulesEngineService
{
    Task<LaneDecision> EvaluateLaneAsync(
        Guid organisationId,
        ConsigneeType consigneeType,
        decimal totalValueGbp,
        bool allProductsHaveCodes,
        CancellationToken cancellationToken = default);

    Task<EligibilityStepResult> AnswerEligibilityQuestionAsync(
        string questionId,
        string answer,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EligibilityQuestion>> GetEligibilityQuestionsAsync(
        CancellationToken cancellationToken = default);

    Task<EligibilityQuestion?> GetFirstEligibilityQuestionAsync(
        CancellationToken cancellationToken = default);

    Task<(string Version, Guid Id)> GetActiveRuleSetInfoAsync(
        CancellationToken cancellationToken = default);
}
