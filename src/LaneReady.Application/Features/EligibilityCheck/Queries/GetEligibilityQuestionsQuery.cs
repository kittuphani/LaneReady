using LaneReady.Application.Common.Interfaces;
using LaneReady.RulesEngine.Models;
using MediatR;

namespace LaneReady.Application.Features.EligibilityCheck.Queries;

public sealed record GetEligibilityQuestionsQuery : IRequest<EligibilityQuestionsDto>;

public record EligibilityQuestionsDto(
    IReadOnlyList<EligibilityQuestion> Questions,
    EligibilityQuestion? FirstQuestion,
    string RuleSetVersion);

public sealed class GetEligibilityQuestionsQueryHandler
    : IRequestHandler<GetEligibilityQuestionsQuery, EligibilityQuestionsDto>
{
    private readonly IRulesEngineService _rulesEngineService;

    public GetEligibilityQuestionsQueryHandler(IRulesEngineService rulesEngineService) =>
        _rulesEngineService = rulesEngineService;

    public async Task<EligibilityQuestionsDto> Handle(
        GetEligibilityQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var questions = await _rulesEngineService.GetEligibilityQuestionsAsync(cancellationToken);
        var first = await _rulesEngineService.GetFirstEligibilityQuestionAsync(cancellationToken);
        var (version, _) = await _rulesEngineService.GetActiveRuleSetInfoAsync(cancellationToken);

        return new EligibilityQuestionsDto(questions, first, version);
    }
}
