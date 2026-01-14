using Bounteous.Data.Extensions.Tests.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.Extensions.Tests.Helpers;

/// <summary>
/// Base class for tests that require DbContext setup.
/// Provides common infrastructure while keeping individual tests readable.
/// </summary>
public abstract class DbContextTestBase : IDisposable
{
    protected DbContextOptions<TestDbContext> DbContextOptions { get; }
    protected Mock<IDbContextObserver> MockObserver { get; }
    protected TestIdentityProvider<long> IdentityProvider { get; }

    protected DbContextTestBase()
    {
        DbContextOptions = TestDbContextFactory.CreateOptions();
        MockObserver = MockObserverFactory.CreateLooseMock();
        IdentityProvider = new TestIdentityProvider<long>();
    }

    /// <summary>
    /// Creates a TestDbContext with the configured options.
    /// </summary>
    protected TestDbContext CreateContext()
    {
        return new TestDbContext(DbContextOptions, MockObserver.Object, IdentityProvider);
    }

    /// <summary>
    /// Creates a TestDbContext with a specific user ID set.
    /// </summary>
    protected TestDbContext CreateContextWithUserId(long userId)
    {
        IdentityProvider.SetCurrentUserId(userId);
        return CreateContext();
    }

    public virtual void Dispose()
    {
        // Cleanup if needed
    }
}
