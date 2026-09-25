using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organisation> Organisations { get; }
    DbSet<User> Users { get; }
    DbSet<Product> Products { get; }
    DbSet<Shipment> Shipments { get; }
    DbSet<ShipmentLine> ShipmentLines { get; }
    DbSet<EvidenceDocument> EvidenceDocuments { get; }
    DbSet<RuleSet> RuleSets { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
