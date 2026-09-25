using System.Text.RegularExpressions;
using LaneReady.Domain.Exceptions;

namespace LaneReady.Domain.ValueObjects;

public sealed record CommodityCode
{
    private static readonly Regex Pattern = new(@"^\d{8}(\d{2})?$", RegexOptions.Compiled);

    public string Value { get; }

    public CommodityCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var cleaned = value.Replace(" ", "").Replace(".", "").Trim();
        if (!Pattern.IsMatch(cleaned))
            throw new DomainException($"'{value}' is not a valid commodity code. Expected 8 or 10 digits.");
        Value = cleaned;
    }

    public string Formatted => Value.Length == 10
        ? $"{Value[..4]} {Value[4..6]} {Value[6..8]} {Value[8..]}"
        : $"{Value[..4]} {Value[4..6]} {Value[6..]}";

    public override string ToString() => Value;
}
