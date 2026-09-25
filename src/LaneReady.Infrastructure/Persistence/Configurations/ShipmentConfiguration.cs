using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ConsigneeType).HasConversion<string>().HasMaxLength(10);
        builder.Property(s => s.Route).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.LaneDecision).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.LaneDecisionReason).HasMaxLength(500);
        builder.Property(s => s.RuleSetVersion).HasMaxLength(50).IsRequired();
        builder.Property(s => s.ApprovalOverrideReason).HasMaxLength(1000);
        builder.Property(s => s.CarrierName).HasMaxLength(100);
        builder.Property(s => s.Notes).HasMaxLength(2000);
        builder.Property(s => s.SourceOrderId).HasMaxLength(100);
        builder.Property(s => s.ApprovedBy).HasMaxLength(100);

        builder.HasMany(s => s.Lines)
            .WithOne()
            .HasForeignKey(l => l.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Evidence)
            .WithOne()
            .HasForeignKey(e => e.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.OrganisationId, s.ShipmentDate });
        builder.HasIndex(s => new { s.OrganisationId, s.Status });
    }
}
