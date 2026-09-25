namespace LaneReady.RulesEngine.Models;

public class EligibilityResult
{
    public bool Eligible { get; set; }
    public string ResultKey { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string Lane { get; set; } = "Unknown";
    public List<string> NextSteps { get; set; } = [];
}
