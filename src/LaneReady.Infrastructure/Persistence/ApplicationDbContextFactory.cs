using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LaneReady.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "LaneReady.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not set.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    private sealed class DesignTimeCurrentUserService
        : LaneReady.Application.Common.Interfaces.ICurrentUserService
    {
        public string? UserId => null;
        public string? UserEmail => null;
        public string? UserFullName => null;
        public Guid? LaneReadyUserId => null;
        public Guid? OrganisationId => null;
        public LaneReady.Domain.Enums.UserRole? UserRole => null;
        public bool IsAuthenticated => false;
        public bool HasOrganisation => false;
        public string? IpAddress => null;
        public bool IsInRole(LaneReady.Domain.Enums.UserRole minimumRole) => false;
    }
}
