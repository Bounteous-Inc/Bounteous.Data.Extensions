using System;

namespace Bounteous.Data.Extensions.Attributes;

/// <summary>
/// Marks methods, classes, or assemblies that should not be used in production code.
/// This attribute can be used by analyzers to warn developers about production usage.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, Inherited = false)]
public sealed class ProductionUsageAttribute : Attribute
{
    /// <summary>
    /// Gets the warning message to display when this is used in production.
    /// </summary>
    public string WarningMessage { get; }

    /// <summary>
    /// Gets whether this should always warn regardless of environment.
    /// </summary>
    public bool AlwaysWarn { get; }

    public ProductionUsageAttribute(string warningMessage = "This API is not intended for production use", bool alwaysWarn = false)
    {
        WarningMessage = warningMessage;
        AlwaysWarn = alwaysWarn;
    }
}
