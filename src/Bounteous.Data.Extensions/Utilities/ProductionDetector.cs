using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Bounteous.Data.Extensions.Utilities;

/// <summary>
/// Utility class to detect if code is running in production, migration, or test contexts.
/// Uses multiple detection strategies with caching for performance.
/// </summary>
public static class ProductionDetector
{
    private static readonly Lazy<bool> _isCachedProduction = new(() => CalculateProductionEnvironment());
    private static readonly Lazy<bool> _isCachedAllowedContext = new(() => CalculateAllowedContext());
    
    /// <summary>
    /// Determines if the current environment is production.
    /// Returns false for migrations and test contexts even in Release configuration.
    /// Uses cached results for performance after first call.
    /// </summary>
    public static bool IsProductionEnvironment => _isCachedProduction.Value;

    /// <summary>
    /// Gets a value indicating whether the current context is explicitly allowed (migration/test).
    /// </summary>
    public static bool IsAllowedContext => _isCachedAllowedContext.Value;

    /// <summary>
    /// Calculates the production environment using multiple detection strategies.
    /// </summary>
    private static bool CalculateProductionEnvironment()
    {
        // Always allow in Debug builds
#if DEBUG
        return false;
#else
        // Strategy 1: Check if explicitly allowed context FIRST (inline to avoid circular dependency)
        if (IsMigrationContext() || IsTestContext() || IsDevelopmentEnvironment())
            return false;

        // Strategy 2: Explicit production environment indicators
        if (IsExplicitProductionEnvironment())
            return true;

        // Strategy 3: Production hosting indicators
        if (IsProductionHostEnvironment())
            return true;

        // Strategy 4: Production assembly indicators
        if (IsProductionAssembly())
            return true;

        // Default to production for Release builds in other contexts
        return true;
#endif
    }

    /// <summary>
    /// Calculates if the current context is explicitly allowed (migration/test).
    /// </summary>
    private static bool CalculateAllowedContext()
    {
        return IsMigrationContext() || IsTestContext() || IsDevelopmentEnvironment();
    }

    /// <summary>
    /// Strategy 1: Checks for explicit production environment variables.
    /// </summary>
    private static bool IsExplicitProductionEnvironment()
    {
        var envVars = new[]
        {
            "ASPNETCORE_ENVIRONMENT",
            "DOTNET_ENVIRONMENT", 
            "ENVIRONMENT",
            "NODE_ENV" // For Node.js interop scenarios
        };

        return envVars.Any(env => 
            Environment.GetEnvironmentVariable(env) == "Production");
    }

    /// <summary>
    /// Strategy 2: Detects production hosting environments.
    /// </summary>
    private static bool IsProductionHostEnvironment()
    {
        // Check common production hosting indicators
        var envVars = new[]
        {
            "AZURE_FUNCTIONS_ENVIRONMENT", // Azure Functions Production
            "AWS_LAMBDA_FUNCTION_NAME",    // AWS Lambda
            "GOOGLE_CLOUD_PROJECT",        // Google Cloud
            "KUBERNETES_SERVICE_HOST",     // Kubernetes
            "HEROKU_APP_NAME",             // Heroku
            "VERCEL_ENV",                  // Vercel
            "NETLIFY_ENV"                  // Netlify
        };

        var hasProductionHosting = envVars.Any(env => 
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(env)));

        // Check for common production hosting process names
        var processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
        var productionProcesses = new[]
        {
            "w3wp",      // IIS
            "iisexpress", // IIS Express (can be production)
            "dotnet",    // .NET runtime (check context)
            "apache2",   // Apache
            "nginx"      // Nginx
        };

        return hasProductionHosting || productionProcesses.Contains(processName);
    }

    /// <summary>
    /// Strategy 3: Detects production assembly patterns.
    /// </summary>
    private static bool IsProductionAssembly()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            return false;

        var assemblyName = entryAssembly.GetName().Name!.ToLowerInvariant();
        
        // Production assembly patterns
        var productionPatterns = new[]
        {
            ".web",
            ".api", 
            ".service",
            ".server",
            ".host",
            "production",
            "prod"
        };

        // Exclude known development/test patterns
        var developmentPatterns = new[]
        {
            ".test",
            ".tests",
            ".integration",
            ".migration",
            ".migrate",
            "debug",
            "dev",
            "development"
        };

        var hasProductionPattern = productionPatterns.Any(pattern => 
            assemblyName.Contains(pattern));
        var hasDevelopmentPattern = developmentPatterns.Any(pattern => 
            assemblyName.Contains(pattern));

        return hasProductionPattern && !hasDevelopmentPattern;
    }

    /// <summary>
    /// Strategy 4: Detects if this is explicitly a development environment.
    /// </summary>
    private static bool IsDevelopmentEnvironment()
    {
        var devEnvVars = new[]
        {
            "ASPNETCORE_ENVIRONMENT",
            "DOTNET_ENVIRONMENT",
            "ENVIRONMENT"
        };

        return devEnvVars.Any(env => 
        {
            var value = Environment.GetEnvironmentVariable(env);
            return value == "Development" || value == "Local" || value == "Dev";
        });
    }

    /// <summary>
    /// Detects if the current context is a data migration.
    /// Enhanced with more detection patterns.
    /// </summary>
    private static bool IsMigrationContext()
    {
        // Check calling assembly for migration indicators
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var assemblyName = entryAssembly.GetName().Name!.ToLowerInvariant();
            
            // Enhanced migration assembly patterns
            var migrationPatterns = new[]
            {
                "migration",
                "migrate", 
                "db.migration",
                "database.migration",
                "ef.migration",
                "entityframework.migration"
            };

            if (migrationPatterns.Any(pattern => assemblyName.Contains(pattern)))
                return true;

            // Check for migration-related types in the call stack
            if (IsMigrationInCallStack())
                return true;
        }

        // Check current process name for EF tools
        var processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
        var efToolProcesses = new[]
        {
            "dotnet-ef",
            "ef",
            "migrate",
            "migration"
        };

        if (efToolProcesses.Any(tool => processName.Contains(tool)))
            return true;

        // Check for EF command line arguments
        try
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.ToLowerInvariant().Contains("migration") || 
                               arg.ToLowerInvariant().Contains("database-update")))
                return true;
        }
        catch
        {
            // If we can't access command line args, continue
        }

        return false;
    }

    /// <summary>
    /// Detects if the current context is a test context.
    /// Enhanced with more test frameworks.
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
            "testhost",
            "vstest",
            "coverlet",
            "moq",
            "nsubstitute",
            "fakit",
            "shouldly",
            "fluentassertions"
        };

        var hasTestAssembly = assemblies.Any(assembly => 
            testIndicators.Any(indicator => 
                assembly.FullName?.Contains(indicator, StringComparison.OrdinalIgnoreCase) == true));

        if (hasTestAssembly)
            return true;

        // Check process name for test runners
        var processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
        var testProcesses = new[]
        {
            "testhost",
            "vstest",
            "nunit",
            "xunit"
        };

        return testProcesses.Any(test => processName.Contains(test));
    }

    /// <summary>
    /// Checks the call stack for migration-related method calls.
    /// Enhanced with more migration patterns.
    /// </summary>
    private static bool IsMigrationInCallStack()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames == null)
            return false;

        var migrationIndicators = new[]
        {
            "migration",
            "migrate",
            "up",
            "down", 
            "dbcontext",
            "modelbuilder",
            "buildmodel",
            "seeddata",
            "configure"
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
