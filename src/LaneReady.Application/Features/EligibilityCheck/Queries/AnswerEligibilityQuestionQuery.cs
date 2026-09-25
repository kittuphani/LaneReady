using FluentValidation;
using LaneReady.Application.Common.Interfaces;
using LaneReady.RulesEngine.Evaluation;
using MediatR;

namespace LaneReady.Application.Features.EligibilityCheck.Queries;

public sealed record AnswerEligibilityQuestionQuery(
    string QuestionId,
    string Answer) : IRequest<EligibilityStepResult>;

public sealed class AnswerEligibilityQuestionQueryValidator
    : AbstractValidator<AnswerEligibilityQuestionQuery>
{
    public AnswerEligibilityQuestionQueryValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(100);
    }
}

public sealed class AnswerEligibilityQuestionQueryHandler
    : IRequestHandler<AnswerEligibilityQuestionQuery, EligibilityStepResult>
{
    private readonly IRulesEngineService _rulesEngineService;

    public AnswerEligibilityQuestionQueryHandler(IRulesEngineService rulesEngineService) =>
        _rulesEngineService = rulesEngineService;

    public Task<EligibilityStepResult> Handle(
        AnswerEligibilityQuestionQuery request,
        CancellationToken cancellationToken) =>
        _rulesEngineService.AnswerEligibilityQuestionAsync(
            request.QuestionId,
            request.Answer,
            cancellationToken);
}
