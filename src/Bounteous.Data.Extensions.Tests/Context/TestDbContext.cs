using Bounteous.Data.Domain.ReadOnly;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Context;

public class TestDbContext : Bounteous.Data.DbContextBase<long>, IDbContext<long>
{
    public TestDbContext(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<long> identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    // Test entities for testing read-only extensions
    public DbSet<TestCompany> Companies { get; set; }
    public DbSet<TestProject> Projects { get; set; }
    public DbSet<ProjectStatus> Statuses { get; set; }
    public DbSet<TestUser> Users { get; set; }

    // Read-only DbSets for testing extensions
    public ReadOnlyDbSet<TestCompany, long> ReadOnlyCompanies => Set<TestCompany>().AsReadOnly<TestCompany, long>();
    public ReadOnlyDbSet<TestProject, long> ReadOnlyProjects => Set<TestProject>().AsReadOnly<TestProject, long>();
    public ReadOnlyDbSet<ProjectStatus, long> ReadOnlyStatuses => Set<ProjectStatus>().AsReadOnly<ProjectStatus, long>();
    public ReadOnlyDbSet<TestUser, long> ReadOnlyUsers => Set<TestUser>().AsReadOnly<TestUser, long>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure test entities
        modelBuilder.Entity<TestCompany>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<TestProject>()
            .Property(p => p.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<ProjectStatus>()
            .Property(s => s.Id)
            .ValueGeneratedNever();
            
        modelBuilder.Entity<TestUser>()
            .Property(u => u.Id)
            .ValueGeneratedNever();
    }
}
