namespace LaneReady.RulesEngine.Models;

public class LaneDecisionRule
{
    public string Id { get; set; } = default!;
    public int Priority { get; set; }
    public string Name { get; set; } = default!;
    public List<RuleCondition> Conditions { get; set; } = [];
    public string Logic { get; set; } = "AND";
    public string Decision { get; set; } = default!;
    public string? Reason { get; set; }
}
