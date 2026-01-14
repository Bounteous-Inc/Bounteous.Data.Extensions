namespace Bounteous.Data.Extensions.Tests.Helpers;

public class TestIdentityProvider<T> : IIdentityProvider<T> where T : struct, IEquatable<T>
{
    private readonly T? currentUserId = null!;
    public T GetCurrentUserId() => currentUserId ?? default;
}
