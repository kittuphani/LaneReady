using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaneReady.Infrastructure.Persistence.Configurations;

public class EvidenceDocumentConfiguration : IEntityTypeConfiguration<EvidenceDocument>
{
    public void Configure(EntityTypeBuilder<EvidenceDocument> builder)
    {
        builder.ToTable("EvidenceDocuments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName).HasMaxLength(260).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(e => e.BlobPath).HasMaxLength(500).IsRequired();
        builder.Property(e => e.BlobContainerName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Sha256Hash).HasMaxLength(64).IsRequired();
        builder.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(e => new { e.OrganisationId, e.ShipmentId });
        builder.HasIndex(e => e.RetainUntil);
    }
}
