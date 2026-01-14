using Bounteous.Data.Extensions.Tests.Context;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Context;

public class TestDbContextFactory : Bounteous.Data.DbContextFactory<TestDbContext, long>
{
    public TestDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer, IIdentityProvider<long> identityProvider)
        : base(connectionBuilder, observer, identityProvider)
    {
    }

    protected override TestDbContext Create(DbContextOptions applyOptions,
        IDbContextObserver dbContextObserver, IIdentityProvider<long> identityProvider)
    {
        return new TestDbContext(applyOptions, dbContextObserver, identityProvider);
    }

    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        if (sensitiveDataLoggingEnabled)
            optionsBuilder.EnableSensitiveDataLogging();

        optionsBuilder.UseInMemoryDatabase("TestDatabase");
        return optionsBuilder.Options;
    }

    /// <summary>
    /// Creates default options for test scenarios.
    /// </summary>
    public static DbContextOptions<TestDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }
}
