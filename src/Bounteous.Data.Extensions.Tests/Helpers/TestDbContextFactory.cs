using Bounteous.Data.Extensions.Tests.Context;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Helpers;

public class TestDbContextFactory(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<long> identityProvider)
    : DbContextFactory<TestDbContext, long>(connectionBuilder, observer, identityProvider)
{
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

    public static DbContextOptions<TestDbContext> CreateOptions()
        => new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
}
