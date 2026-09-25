using LaneReady.Domain.Common;
using LaneReady.Domain.ValueObjects;

namespace LaneReady.Domain.Entities;

public sealed class ShipmentLine : TenantedAuditableEntity
{
    public Guid ShipmentId { get; private set; }
    public string ProductSku { get; private set; } = default!;
    public string ProductDescription { get; private set; } = default!;
    public string CommodityCode { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money LineValue { get; private set; } = default!;
    public decimal GrossWeightKg { get; private set; }
    public decimal NetWeightKg { get; private set; }
    public string CountryOfOrigin { get; private set; } = default!;

    private ShipmentLine() { }

    public static ShipmentLine CreateFromProduct(
        Guid organisationId,
        Guid shipmentId,
        Product product,
        int quantity)
    {
        return new ShipmentLine
        {
            OrganisationId = organisationId,
            ShipmentId = shipmentId,
            ProductSku = product.Sku,
            ProductDescription = product.Description,
            CommodityCode = product.ConfirmedCommodityCode?.Value ?? string.Empty,
            Quantity = quantity,
            LineValue = new Money(product.UnitValue.Amount * quantity, product.UnitValue.Currency),
            GrossWeightKg = product.GrossWeightKg * quantity,
            NetWeightKg = product.NetWeightKg * quantity,
            CountryOfOrigin = product.CountryOfOrigin
        };
    }
}
