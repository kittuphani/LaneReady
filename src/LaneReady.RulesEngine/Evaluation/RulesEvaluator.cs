using LaneReady.Domain.Enums;
using LaneReady.RulesEngine.Models;

namespace LaneReady.RulesEngine.Evaluation;

public class RulesEvaluator : IRulesEvaluator
{
    public LaneDecision EvaluateLane(EvaluationContext context, RuleSetDefinition rules)
    {
        foreach (var rule in rules.LaneDecisionRules.OrderByDescending(r => r.Priority))
        {
            if (rule.Logic == "DEFAULT" || EvaluateConditions(rule, context))
            {
                return Enum.TryParse<LaneDecision>(rule.Decision, true, out var decision)
                    ? decision
                    : LaneDecision.Red;
            }
        }
        return LaneDecision.Red;
    }

    public EligibilityStepResult AnswerQuestion(string questionId, string answer, RuleSetDefinition rules)
    {
        var question = rules.EligibilityQuestions.SingleOrDefault(q => q.Id == questionId)
            ?? throw new InvalidOperationException($"Question '{questionId}' not found in rule set.");

        var normalised = answer.Trim().ToLowerInvariant();
        if (!question.Branch.TryGetValue(normalised, out var next))
        {
            next = question.Branch.TryGetValue("default", out var def) ? def : null;
        }

        if (next is null)
            return new EligibilityStepResult(null, new EligibilityResult
            {
                Eligible = false,
                ResultKey = "unknown",
                Message = "Unable to determine a result based on this answer. Please consult a customs adviser.",
                Lane = "Unknown"
            });

        if (next.StartsWith("result:", StringComparison.OrdinalIgnoreCase))
        {
            var resultKey = next["result:".Length..];
            if (rules.EligibilityResults.TryGetValue(resultKey, out var result))
            {
                result.ResultKey = resultKey;
                return new EligibilityStepResult(null, result);
            }
            return new EligibilityStepResult(null, new EligibilityResult
            {
                Eligible = false,
                ResultKey = resultKey,
                Message = "Result configuration not found.",
                Lane = "Unknown"
            });
        }

        var nextQuestion = rules.EligibilityQuestions.FirstOrDefault(q => q.Id == next);
        return new EligibilityStepResult(nextQuestion, null);
    }

    public EligibilityQuestion? GetFirstQuestion(RuleSetDefinition rules) =>
        rules.EligibilityQuestions.OrderBy(q => q.Order).FirstOrDefault();

    private static bool EvaluateConditions(LaneDecisionRule rule, EvaluationContext ctx)
    {
        if (rule.Conditions.Count == 0) return true;

        return rule.Logic.ToUpperInvariant() switch
        {
            "AND" => rule.Conditions.All(c => ConditionEvaluator.Evaluate(c, ctx)),
            "OR" => rule.Conditions.Any(c => ConditionEvaluator.Evaluate(c, ctx)),
            _ => rule.Conditions.All(c => ConditionEvaluator.Evaluate(c, ctx))
        };
    }
}
