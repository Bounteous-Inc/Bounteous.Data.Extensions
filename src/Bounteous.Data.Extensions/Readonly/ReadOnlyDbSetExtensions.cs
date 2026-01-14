using Bounteous.Data.Domain.Interfaces;
using Bounteous.Data.Domain.ReadOnly;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Readonly;

/// <summary>
/// Extension methods for DbSet to create ReadOnlyDbSet instances.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Converts a DbSet to a ReadOnlyDbSet wrapper.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <param name="dbSet">The DbSet to convert.</param>
    /// <returns>A ReadOnlyDbSet wrapper.</returns>
    public static ReadOnlyDbSet<T, TKey> AsReadOnly<T, TKey>(this DbSet<T> dbSet) 
        where T : class, IReadOnlyEntity<TKey>
    {
        return new ReadOnlyDbSet<T, TKey>(dbSet);
    }
}

/// <summary>
/// ⚠️ **NOT INTENDED FOR PRODUCTION USE** ⚠️
/// 
/// Extension methods for ReadOnlyDbSet to enable creation of test objects in unit tests and data migrations.
/// This package bypasses read-only validation and should only be used in:
/// - Unit test projects
/// - Data migration projects  
/// - Development/Integration test environments
/// 
/// **DO NOT REFERENCE THIS PACKAGE IN PRODUCTION CODE**
/// </summary>
public static class ReadOnlyDbSetExtensions
{
    /// <summary>
    /// ⚠️ **PRODUCTION WARNING**: This method bypasses read-only validation.
    /// Use only in testing or migration scenarios, never in production code.
    /// 
    /// Creates an entity using the provided factory function and adds it to the ReadOnlyDbSet.
    /// This method bypasses read-only validation to enable test data creation.
    /// </summary>
    /// <param name="readOnlyDbSet"></param>
    /// <returns>The created entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entityFactory is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when used in production environment.</exception>
    public static System.Threading.Tasks.Task<T> CreateAsync<T, TKey>(this ReadOnlyDbSet<T, TKey> readOnlyDbSet, System.Func<T> entityFactory)
        where T : class, IReadOnlyEntity<TKey>
    {
        ThrowIfInProduction();
        var innerDbSet = GetInnerDbSet(readOnlyDbSet);
        var entity = entityFactory();
        innerDbSet.Add(entity);
        return System.Threading.Tasks.Task.FromResult(entity);
    }

    /// <summary>
    /// ⚠️ **PRODUCTION WARNING**: This method bypasses read-only validation.
    /// Use only in testing or migration scenarios, never in production code.
    /// 
    /// Creates an array of entities using the provided factory function and adds them to the ReadOnlyDbSet.
    /// This method bypasses read-only validation to enable bulk test data creation with array return type.
    /// </summary>
    /// <param name="entityFactory">A factory function that creates an array of entities.</param>
    /// <returns>The array of created entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when entityFactory is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when used in production environment.</exception>
    public static System.Threading.Tasks.Task<System.Collections.Generic.List<T>> CreateAsync<T, TKey>(this ReadOnlyDbSet<T, TKey> readOnlyDbSet, System.Func<System.Collections.Generic.List<T>> entityFactory)
        where T : class, IReadOnlyEntity<TKey>
    {
        ThrowIfInProduction();
        var innerDbSet = GetInnerDbSet(readOnlyDbSet);
        var entities = entityFactory();
        foreach (var entity in entities)
            innerDbSet.Add(entity);
        return System.Threading.Tasks.Task.FromResult(entities);
    }

    private static DbSet<T> GetInnerDbSet<T, TKey>(ReadOnlyDbSet<T, TKey> readOnlyDbSet)
        where T : class, IReadOnlyEntity<TKey>
    {
        var innerDbSetProperty = readOnlyDbSet.GetType().GetProperty("InnerDbSet",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (innerDbSetProperty?.GetValue(readOnlyDbSet) is DbSet<T> innerDbSet)
            return innerDbSet;

        // Fallback: Try to find any DbSet<T> field or property
        var fields = readOnlyDbSet.GetType()
            .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
            if (field.FieldType == typeof(DbSet<T>))
                return (DbSet<T>)field.GetValue(readOnlyDbSet)!;

        var properties = readOnlyDbSet.GetType()
            .GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var prop in properties)
            if (prop.PropertyType == typeof(DbSet<T>))
                return (DbSet<T>)prop.GetValue(readOnlyDbSet)!;

        throw new System.InvalidOperationException(
            "Unable to access InnerDbSet property from ReadOnlyDbSet. The ReadOnlyDbSet structure may have changed.");
    }
    
    /// <summary>
    /// Throws an exception if the current environment is production.
    /// This prevents accidental usage of developer utilities in production.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when running in production environment.</exception>
    private static void ThrowIfInProduction()
    {
        var isProduction = 
#if DEBUG
            false;
#else
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ||
        Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Production";
#endif

        if (isProduction)
        {
            throw new System.InvalidOperationException(
                "Bounteous.Data.Extensions is not intended for production use. " +
                "This package bypasses read-only validation and should only be used in testing, " +
                "migration, or development scenarios. " +
                "Remove this package reference from production projects.");
        }
    }
}