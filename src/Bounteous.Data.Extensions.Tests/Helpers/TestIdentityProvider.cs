namespace Bounteous.Data.Extensions.Tests.Helpers;

/// <summary>
/// Test implementation of IIdentityProvider for unit testing.
/// </summary>
/// <typeparam name="T">The type of the user identifier.</typeparam>
public class TestIdentityProvider<T> : IIdentityProvider<T> where T : struct, IEquatable<T>
{
    private T? _currentUserId;

    /// <summary>
    /// Gets the current user identifier.
    /// </summary>
    public T? CurrentUserId => _currentUserId;

    /// <summary>
    /// Gets the current user identifier (implementation for interface).
    /// </summary>
    public T GetCurrentUserId() => _currentUserId ?? default;

    /// <summary>
    /// Sets the current user identifier for testing.
    /// </summary>
    /// <param name="userId">The user identifier to set.</param>
    public void SetCurrentUserId(T userId)
    {
        _currentUserId = userId;
    }

    /// <summary>
    /// Clears the current user identifier.
    /// </summary>
    public void ClearCurrentUserId()
    {
        _currentUserId = null;
    }
}
