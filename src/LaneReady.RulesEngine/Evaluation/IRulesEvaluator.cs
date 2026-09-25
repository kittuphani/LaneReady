using LaneReady.Domain.Enums;
using LaneReady.RulesEngine.Models;

namespace LaneReady.RulesEngine.Evaluation;

public interface IRulesEvaluator
{
    LaneDecision EvaluateLane(EvaluationContext context, RuleSetDefinition rules);
    EligibilityStepResult AnswerQuestion(string questionId, string answer, RuleSetDefinition rules);
    EligibilityQuestion? GetFirstQuestion(RuleSetDefinition rules);
}

public class EligibilityStepResult
{
    public EligibilityQuestion? NextQuestion { get; }
    public EligibilityResult? Result { get; }
    public bool IsComplete => Result is not null;

    public EligibilityStepResult(EligibilityQuestion? nextQuestion, EligibilityResult? result)
    {
        NextQuestion = nextQuestion;
        Result = result;
    }
}
