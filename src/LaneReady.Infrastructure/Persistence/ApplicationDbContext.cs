using LaneReady.Application.Common.Interfaces;
using LaneReady.Domain.Common;
using LaneReady.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaneReady.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<EvidenceDocument> EvidenceDocuments => Set<EvidenceDocument>();
    public DbSet<RuleSet> RuleSets => Set<RuleSet>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Evaluated at query time per request — not baked in at model build time
    private Guid CurrentTenantId => _currentUserService.OrganisationId ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyGlobalQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Only tenanted entities get a filter; Organisation, RuleSet, and AuditLog are not filtered
        modelBuilder.Entity<User>()
            .HasQueryFilter(e => e.OrganisationId == CurrentTenantId);

        modelBuilder.Entity<Product>()
            .HasQueryFilter(e => e.OrganisationId == CurrentTenantId);

        modelBuilder.Entity<Shipment>()
            .HasQueryFilter(e => e.OrganisationId == CurrentTenantId);

        modelBuilder.Entity<ShipmentLine>()
            .HasQueryFilter(e => e.OrganisationId == CurrentTenantId);

        modelBuilder.Entity<EvidenceDocument>()
            .HasQueryFilter(e => e.OrganisationId == CurrentTenantId);
    }
}
