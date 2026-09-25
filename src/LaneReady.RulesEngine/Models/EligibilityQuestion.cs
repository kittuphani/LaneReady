namespace LaneReady.RulesEngine.Models;

public class EligibilityQuestion
{
    public string Id { get; set; } = default!;
    public int Order { get; set; }
    public string Text { get; set; } = default!;
    public string? HelpText { get; set; }
    public string Type { get; set; } = "boolean";
    public Dictionary<string, string> Branch { get; set; } = [];
    public List<string>? Options { get; set; }
}
