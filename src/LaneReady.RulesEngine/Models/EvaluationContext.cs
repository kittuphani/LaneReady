namespace LaneReady.RulesEngine.Models;

public class EvaluationContext
{
    public OrganisationContext Organisation { get; init; } = new();
    public ShipmentContext Shipment { get; init; } = new();

    public object? Resolve(string dotPath)
    {
        var parts = dotPath.Split('.', 2);
        if (parts.Length != 2) return null;

        return parts[0].ToLowerInvariant() switch
        {
            "organisation" => ResolveOrganisation(parts[1]),
            "shipment" => ResolveShipment(parts[1]),
            _ => null
        };
    }

    private object? ResolveOrganisation(string field) => field.ToLowerInvariant() switch
    {
        "hasukims" => Organisation.HasUkims,
        "ukimsexpired" => Organisation.UkimsExpired,
        "plantier" => Organisation.PlanTier,
        "isb2c" => Organisation.IsB2C,
        _ => null
    };

    private object? ResolveShipment(string field) => field.ToLowerInvariant() switch
    {
        "consigneetype" => Shipment.ConsigneeType,
        "totalvaluegbp" => Shipment.TotalValueGbp,
        "allproductshavecodes" => Shipment.AllProductsHaveCodes,
        "linecount" => Shipment.LineCount,
        _ => null
    };
}

public class OrganisationContext
{
    public bool HasUkims { get; init; }
    public bool UkimsExpired { get; init; }
    public string PlanTier { get; init; } = string.Empty;
    public bool IsB2C { get; init; }
}

public class ShipmentContext
{
    public string ConsigneeType { get; init; } = string.Empty;
    public decimal TotalValueGbp { get; init; }
    public bool AllProductsHaveCodes { get; init; }
    public int LineCount { get; init; }
}
