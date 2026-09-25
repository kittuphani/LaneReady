using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("Organisations");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(o => o.GbEori, eori =>
            eori.Property(e => e.Value)
                .HasColumnName("GbEoriNumber")
                .HasMaxLength(20)
                .IsRequired());

        builder.OwnsOne(o => o.XiEori, eori =>
            eori.Property(e => e.Value)
                .HasColumnName("XiEoriNumber")
                .HasMaxLength(20));

        builder.OwnsOne(o => o.Vat, vat =>
            vat.Property(v => v.Value)
                .HasColumnName("VatNumber")
                .HasMaxLength(20));

        builder.Property(o => o.UkimsNumber).HasMaxLength(50);
        builder.Property(o => o.Plan).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.StripeCustomerId).HasMaxLength(100);
        builder.Property(o => o.StripeSubscriptionId).HasMaxLength(100);

        builder.HasIndex(o => o.StripeCustomerId);
    }
}
