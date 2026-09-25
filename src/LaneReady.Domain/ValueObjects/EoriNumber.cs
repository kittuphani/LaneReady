using System.Text.RegularExpressions;
using LaneReady.Domain.Exceptions;

namespace LaneReady.Domain.ValueObjects;

public sealed record EoriNumber
{
    private static readonly Regex GbPattern = new(@"^GB\d{12}$", RegexOptions.Compiled);
    private static readonly Regex XiPattern = new(@"^XI\d{12}$", RegexOptions.Compiled);

    public string Value { get; }

    public EoriNumber(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var upper = value.Trim().ToUpperInvariant();
        if (!GbPattern.IsMatch(upper) && !XiPattern.IsMatch(upper))
            throw new DomainException($"'{value}' is not a valid EORI number. Expected GB or XI followed by 12 digits.");
        Value = upper;
    }

    public bool IsXiEori => Value.StartsWith("XI", StringComparison.Ordinal);
    public bool IsGbEori => Value.StartsWith("GB", StringComparison.Ordinal);

    public override string ToString() => Value;
}
