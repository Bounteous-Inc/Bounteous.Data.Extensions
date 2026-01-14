using Moq;

namespace Bounteous.Data.Extensions.Tests.Helpers;

public static class MockObserverFactory
{
    public static Mock<IDbContextObserver> CreateLooseMock()
     =>  new(MockBehavior.Loose);
}
