using Bounteous.Data.Domain.ReadOnly;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Context;

public class TestDbContext : DbContextBase<long>
{
    public TestDbContext(DbContextOptions options, IDbContextObserver observer, IIdentityProvider<long> identityProvider)
        : base(options, observer, identityProvider)
    {
    }

    // Read-only DbSets for testing extensions
    public ReadOnlyDbSet<TestCompany, long> ReadOnlyCompanies => 
        Set<TestCompany>().AsReadOnly<TestCompany, long>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        // Configure test entities
        modelBuilder.Entity<TestCompany>()
            .Property(c => c.Id)
            .ValueGeneratedNever();
    }
}
