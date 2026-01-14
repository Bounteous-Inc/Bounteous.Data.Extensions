using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bounteous.Data.Extensions.Utilities;

/// <summary>
/// Utility class to detect if code is running in production, migration, or test contexts.
/// </summary>
public static class ProductionDetector
{
    /// <summary>
    /// Determines if the current environment is production.
    /// Returns false for migrations and test contexts even in Release configuration.
    /// </summary>
    public static bool IsProductionEnvironment()
    {
        // Always allow in Debug builds
#if DEBUG
        return false;
#else
        // Check for explicit production environment variables
        if (IsExplicitProductionEnvironment())
            return true;

        // Allow in migration contexts
        if (IsMigrationContext())
            return false;

        // Allow in test contexts
        if (IsTestContext())
            return false;

        // Default to production for Release builds in other contexts
        return true;
#endif
    }

    /// <summary>
    /// Checks for explicit production environment variables.
    /// </summary>
    private static bool IsExplicitProductionEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ||
               Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Production";
    }

    /// <summary>
    /// Detects if the current context is a data migration.
    /// </summary>
    private static bool IsMigrationContext()
    {
        // Check calling assembly for migration indicators
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var assemblyName = entryAssembly.GetName().Name!;
            
            // Common migration assembly patterns
            if (assemblyName.Contains("Migration", StringComparison.OrdinalIgnoreCase) ||
                assemblyName.Contains("Migrate", StringComparison.OrdinalIgnoreCase) ||
                assemblyName.EndsWith(".Migrations", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for migration-related types in the call stack
            return IsMigrationInCallStack();
        }

        // Check current process name
        var processName = Process.GetCurrentProcess().ProcessName;
        if (processName.Contains("dotnet-ef", StringComparison.OrdinalIgnoreCase) ||
            processName.Contains("ef", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Detects if the current context is a test context.
    /// </summary>
    private static bool IsTestContext()
    {
        // Check if test frameworks are loaded
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        var testIndicators = new[]
        {
            "xunit",
            "nunit",
            "mstest",
            "unittest",
            "testhost"
        };

        return assemblies.Any(assembly => 
            testIndicators.Any(indicator => 
                assembly.FullName?.Contains(indicator, StringComparison.OrdinalIgnoreCase) == true));
    }

    /// <summary>
    /// Checks the call stack for migration-related method calls.
    /// </summary>
    private static bool IsMigrationInCallStack()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames == null)
            return false;

        var migrationIndicators = new[]
        {
            "Migration",
            "Migrate",
            "Up",
            "Down",
            "DbContext",
            "ModelBuilder"
        };

        return frames
            .Select(frame => frame.GetMethod())
            .Where(method => method != null)
            .Any(method => 
                migrationIndicators.Any(indicator => 
                    method!.Name?.Contains(indicator, StringComparison.OrdinalIgnoreCase) == true ||
                    method!.DeclaringType?.Name?.Contains(indicator, StringComparison.OrdinalIgnoreCase) == true));
    }
}
