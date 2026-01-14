using System;
using Bounteous.Data.Extensions.Attributes;
using Bounteous.Data.Extensions.Utilities;

namespace Bounteous.Data.Extensions;

/// <summary>
/// ⚠️ **NOT INTENDED FOR PRODUCTION USE** ⚠️
/// 
/// This namespace contains extension methods and utilities that bypass normal 
/// data validation patterns. These are specifically designed for:
/// - Unit testing scenarios
/// - Database seeding in migrations
/// - Development and integration testing
/// 
/// **PRODUCTION USAGE IS STRICTLY PROHIBITED**
/// 
/// The methods in this namespace will throw exceptions in production environments
/// unless explicitly allowed in migration or test contexts.
/// </summary>
[ProductionUsage("This namespace contains development utilities that should not be used in production code")]
public static class ProductionWarningMarker
{
    /// <summary>
    /// Validates that the current context is appropriate for using development utilities.
    /// Throws an exception if used in production code (except migrations and tests).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when running in production environment.</exception>
    public static void ValidateContext()
    {
        if (ProductionDetector.IsProductionEnvironment())
        {
            throw new InvalidOperationException(
                "Bounteous.Data.Extensions development utilities are not intended for production use. " +
                "This package bypasses read-only validation and should only be used in testing, " +
                "migration, or development scenarios. " +
                "Remove this package reference from production projects.");
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current environment is production.
    /// Returns false for migrations and test contexts even in Release configuration.
    /// </summary>
    public static bool IsProductionEnvironment => ProductionDetector.IsProductionEnvironment();
}