using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class ShipmentLineConfiguration : IEntityTypeConfiguration<ShipmentLine>
{
    public void Configure(EntityTypeBuilder<ShipmentLine> builder)
    {
        builder.ToTable("ShipmentLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ProductSku).HasMaxLength(100).IsRequired();
        builder.Property(l => l.ProductDescription).HasMaxLength(500).IsRequired();
        builder.Property(l => l.CommodityCode).HasMaxLength(10);
        builder.Property(l => l.CountryOfOrigin).HasMaxLength(3);
        builder.Property(l => l.GrossWeightKg).HasPrecision(10, 4);
        builder.Property(l => l.NetWeightKg).HasPrecision(10, 4);

        builder.OwnsOne(l => l.LineValue, mv =>
        {
            mv.Property(m => m.Amount).HasColumnName("LineValueAmount").HasPrecision(18, 4);
            mv.Property(m => m.Currency).HasColumnName("LineValueCurrency").HasMaxLength(3);
        });

        builder.HasIndex(l => new { l.ShipmentId, l.OrganisationId });
    }
}
