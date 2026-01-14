using AwesomeAssertions;
using Bounteous.Data.Extensions.Readonly;
using Bounteous.Data.Extensions.Tests.Context;
using Bounteous.Data.Extensions.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Integration;

[Collection("Bounteous")]
public class ReadOnlyBehaviorTests : DbContextTestBase
{
    private readonly TestDbContext context;

    public ReadOnlyBehaviorTests()
    {
        context = CreateContext();
    }

    [Fact]
    public void ReadOnlyDbSet_ShouldPreventDirectModification()
    {
        // Arrange
        var readOnlyCompanies = context.ReadOnlyCompanies;
        var company = new TestCompany { Name = "Test", Code = "TEST" };

        // Act & Assert
        // These operations should throw ReadOnlyEntityException
        Assert.Throws<Bounteous.Data.Exceptions.ReadOnlyEntityException>(() => readOnlyCompanies.Add(company));
        Assert.Throws<Bounteous.Data.Exceptions.ReadOnlyEntityException>(() => readOnlyCompanies.Remove(company));
        Assert.Throws<Bounteous.Data.Exceptions.ReadOnlyEntityException>(() => readOnlyCompanies.Update(company));
    }

    [Fact]
    public async Task ReadOnlyDbSet_ShouldAllowReadOnlyOperations()
    {
        // Arrange
        using var validationScope = context.SuppressReadOnlyValidation();
        var company = new TestCompany 
        { 
            Id = 1L,
            Name = "Test Company", 
            Code = "TEST" 
        };
        
        await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();

        // Act - Use the underlying DbSet for read operations through ReadOnlyDbSet
        var allCompanies = await context.Companies.ToListAsync();

        // Assert
        allCompanies.Should().HaveCount(1);
        allCompanies[0].Name.Should().Be("Test Company");
    }

    [Fact]
    public async Task CreateAsync_WithSuppressValidation_ShouldWork()
    {
        // Arrange
        using var validationScope = context.SuppressReadOnlyValidation();
        var company = new TestCompany 
        { 
            Name = "New Company", 
            Code = "NEW-001" 
        };

        // Act
        var createdCompany = await Bounteous.Data.Extensions.Readonly.ReadOnlyDbSetExtensions.CreateAsync(
            context.ReadOnlyCompanies, () => company);
        await context.SaveChangesAsync();

        // Assert
        createdCompany.Should().NotBeNull();
        createdCompany.Name.Should().Be("New Company");
        
        // Verify it was actually saved
        var savedCompany = await context.Companies.FindAsync(createdCompany.Id);
        savedCompany.Should().NotBeNull();
        savedCompany.Name.Should().Be("New Company");
    }

    public override void Dispose()
    {
        context.Dispose();
        base.Dispose();
    }
}
