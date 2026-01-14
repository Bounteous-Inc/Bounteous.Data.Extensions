using Bounteous.Data.Domain.Entities;

namespace Bounteous.Data.Extensions.Tests.Context;

/// <summary>
/// Test company entity for testing read-only extensions.
/// </summary>
public class TestCompany : ReadOnlyEntityBase<long>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}