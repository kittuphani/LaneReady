namespace LaneReady.RulesEngine.Models;

public class RuleSetDefinition
{
    public string Version { get; set; } = default!;
    public string Schema { get; set; } = "1.0";
    public List<EligibilityQuestion> EligibilityQuestions { get; set; } = [];
    public Dictionary<string, EligibilityResult> EligibilityResults { get; set; } = [];
    public List<LaneDecisionRule> LaneDecisionRules { get; set; } = [];
    public List<string> NirmsChapterPrefixes { get; set; } = [];
    public List<string> VagueDescriptionTerms { get; set; } = [];
}
