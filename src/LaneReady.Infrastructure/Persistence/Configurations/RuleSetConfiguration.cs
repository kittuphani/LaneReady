using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class RuleSetConfiguration : IEntityTypeConfiguration<RuleSet>
{
    public void Configure(EntityTypeBuilder<RuleSet> builder)
    {
        builder.ToTable("RuleSets");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Version).HasMaxLength(50).IsRequired();
        builder.Property(r => r.RulesJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(r => r.AuthoredBy).HasMaxLength(100);
        builder.Property(r => r.ApprovedBy).HasMaxLength(100);

        builder.HasIndex(r => r.Version).IsUnique();
        builder.HasIndex(r => r.IsActive);
        builder.HasIndex(r => r.EffectiveFrom);
    }
}
