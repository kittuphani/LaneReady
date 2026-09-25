using System.Text.RegularExpressions;
using LaneReady.Domain.Exceptions;

namespace LaneReady.Domain.ValueObjects;

public sealed record VatNumber
{
    private static readonly Regex GbPattern = new(@"^(GB)?(\d{9}|\d{12})$", RegexOptions.Compiled);

    public string Value { get; }

    public VatNumber(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var cleaned = value.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        if (!GbPattern.IsMatch(cleaned))
            throw new DomainException($"'{value}' is not a valid UK VAT number. Expected 9 or 12 digits, optionally prefixed with GB.");
        Value = cleaned.StartsWith("GB", StringComparison.Ordinal) ? cleaned : $"GB{cleaned}";
    }

    public override string ToString() => Value;
}
