using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Readonly;

/// <summary>
/// ⚠️ **NOT INTENDED FOR PRODUCTION USE** ⚠️
/// 
/// Extension methods for DbContext to enable suppression of read-only validation in testing and migration scenarios.
/// This provides a convenient way to access the Bounteous.Data ReadOnlyValidationScope functionality.
/// 
/// **DO NOT REFERENCE THIS PACKAGE IN PRODUCTION CODE**
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// ⚠️ **PRODUCTION WARNING**: This method bypasses read-only validation.
    /// Use only in testing or migration scenarios, never in production code.
    /// 
    /// Creates a scope where read-only entity validation is suppressed for this DbContext.
    /// This uses the Bounteous.Data ReadOnlyValidationScope to suppress validation.
    /// </summary>
    /// <param name="context">The DbContext instance (unused, but required for extension method syntax).</param>
    /// <returns>An IDisposable that must be disposed to re-enable validation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when used in production environment.</exception>
    public static IDisposable SuppressReadOnlyValidation(this DbContext context)
    {
        // Use the Bounteous.Data ReadOnlyValidationScope to suppress validation
        return new Bounteous.Data.ReadOnlyValidationScope();
    }

    /// <summary>
    /// ⚠️ **PRODUCTION WARNING**: This method bypasses read-only validation.
    /// Use only in testing or migration scenarios, never in production code.
    /// 
    /// Alias for SuppressReadOnlyValidation() - creates a scope where read-only entity validation is suppressed.
    /// This method exists for backward compatibility with existing test code.
    /// </summary>
    /// <param name="context">The DbContext instance (unused, but required for extension method syntax).</param>
    /// <returns>An IDisposable that must be disposed to re-enable validation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when used in production environment.</exception>
    public static IDisposable AllowTestSeeding(this DbContext context)
    {
        return context.SuppressReadOnlyValidation();
    }
}
