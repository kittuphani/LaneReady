using System.Text.Json;
using System.Text.Json.Serialization;
using LaneReady.RulesEngine.Models;

namespace LaneReady.RulesEngine.Serialization;

public static class RuleSetSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static RuleSetDefinition Deserialize(string json)
    {
        return JsonSerializer.Deserialize<RuleSetDefinition>(json, Options)
            ?? throw new InvalidOperationException("Failed to deserialize rule set JSON.");
    }

    public static string Serialize(RuleSetDefinition definition) =>
        JsonSerializer.Serialize(definition, Options);

    public static bool TryDeserialize(string json, out RuleSetDefinition? definition, out string? error)
    {
        try
        {
            definition = Deserialize(json);
            error = null;
            return true;
        }
        catch (JsonException ex)
        {
            definition = null;
            error = ex.Message;
            return false;
        }
    }
}
