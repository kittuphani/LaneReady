namespace LaneReady.RulesEngine.Models;

public class RuleCondition
{
    public string Field { get; set; } = default!;
    public string Op { get; set; } = "eq";
    public object? Value { get; set; }
}
