using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ActorUserId).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ActorEmail).HasMaxLength(320);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.BeforeJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.AfterJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.AdditionalInfo).HasMaxLength(1000);

        builder.HasIndex(a => new { a.OrganisationId, a.Timestamp });
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.ActorUserId);
    }
}
