using Bounteous.Core.Extensions;
using Bounteous.Data.Extensions.Tests.Factory;

namespace Bounteous.Data.Extensions.Tests.Fixtures;

[CollectionDefinition("Bounteous")]
public class TestCollection : ICollectionFixture<TestFixture>;

public class TestFixture : IDisposable
{
    public TestFixture()
        => new CleanFactory()
            .Then(new EntityFractory())
            .Run();

    public void Dispose() => new CleanFactory().Run();
}