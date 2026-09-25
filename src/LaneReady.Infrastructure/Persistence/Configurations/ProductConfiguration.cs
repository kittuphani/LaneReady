using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Title).HasMaxLength(500).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.CountryOfOrigin).HasMaxLength(3);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.GrossWeightKg).HasPrecision(10, 4);
        builder.Property(p => p.NetWeightKg).HasPrecision(10, 4);

        builder.OwnsOne(p => p.ConfirmedCommodityCode, cc =>
            cc.Property(c => c.Value).HasColumnName("CommodityCode").HasMaxLength(10));

        builder.OwnsOne(p => p.UnitValue, mv =>
        {
            mv.Property(m => m.Amount).HasColumnName("UnitValueAmount").HasPrecision(18, 4);
            mv.Property(m => m.Currency).HasColumnName("UnitValueCurrency").HasMaxLength(3);
        });

        builder.OwnsOne(p => p.AiSuggestion, ai =>
        {
            ai.OwnsOne(a => a.SuggestedCommodityCode, cc =>
                cc.Property(c => c.Value).HasColumnName("AiCommodityCode").HasMaxLength(10));
            ai.Property(a => a.SuggestedDescription).HasColumnName("AiDescription").HasMaxLength(2000);
            ai.Property(a => a.ConfidenceScore).HasColumnName("AiConfidence").HasPrecision(5, 4);
            ai.Property(a => a.Reasoning).HasColumnName("AiReasoning").HasMaxLength(2000);
            ai.Property(a => a.ModelVersion).HasColumnName("AiModelVersion").HasMaxLength(100);
        });

        builder.Property(p => p.AiSuggestionConfirmedBy).HasMaxLength(100);

        builder.HasIndex(p => new { p.OrganisationId, p.Sku }).IsUnique();
        builder.HasIndex(p => new { p.OrganisationId, p.Status });
    }
}
