using LaneReady.Domain.Exceptions;
using LaneReady.RulesEngine.Models;

namespace LaneReady.RulesEngine.Evaluation;

public class ConditionEvaluator
{
    public static bool Evaluate(RuleCondition condition, EvaluationContext ctx)
    {
        var actual = ctx.Resolve(condition.Field);
        return condition.Op switch
        {
            "eq" => AreEqual(actual, condition.Value),
            "neq" => !AreEqual(actual, condition.Value),
            "gt" => Compare(actual, condition.Value) > 0,
            "gte" => Compare(actual, condition.Value) >= 0,
            "lt" => Compare(actual, condition.Value) < 0,
            "lte" => Compare(actual, condition.Value) <= 0,
            "in" => IsIn(actual, condition.Value),
            _ => throw new DomainException($"Unknown condition operator: '{condition.Op}'")
        };
    }

    private static bool AreEqual(object? actual, object? expected)
    {
        if (actual is null && expected is null) return true;
        if (actual is null || expected is null) return false;

        if (actual is bool bActual && expected is bool bExpected)
            return bActual == bExpected;

        if (expected is bool expectedBool)
        {
            if (actual is string s && bool.TryParse(s, out var parsedBool))
                return parsedBool == expectedBool;
        }

        return string.Equals(actual.ToString(), expected.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static int Compare(object? actual, object? expected)
    {
        if (actual is null || expected is null)
            throw new DomainException("Cannot compare null values.");

        if (actual is decimal dActual)
        {
            var dExpected = Convert.ToDecimal(expected, System.Globalization.CultureInfo.InvariantCulture);
            return dActual.CompareTo(dExpected);
        }

        if (actual is int iActual)
            return iActual.CompareTo(Convert.ToInt32(expected, System.Globalization.CultureInfo.InvariantCulture));

        throw new DomainException($"Cannot compare type '{actual.GetType().Name}'.");
    }

    private static bool IsIn(object? actual, object? values)
    {
        if (actual is null || values is null) return false;
        if (values is not IEnumerable<object> list) return false;
        return list.Any(v => AreEqual(actual, v));
    }
}
