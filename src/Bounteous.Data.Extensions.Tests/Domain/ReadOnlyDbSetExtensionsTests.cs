using AwesomeAssertions;
using Bounteous.Core.Validations;
using Bounteous.Data.Extensions.Readonly;
using Bounteous.Data.Extensions.Tests.Context;
using Bounteous.Data.Extensions.Tests.Helpers;
using Bounteous.xUnit.Accelerator.Factory;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.Extensions.Tests.Domain;

[Collection("Bounteous")]
public class ReadOnlyDbSetExtensionsTests : DbContextTestBase
{
    private readonly TestDbContext context;
    private readonly IDisposable readonlyScope;

    public ReadOnlyDbSetExtensionsTests()
    {
        context = CreateContext();
        readonlyScope = context.SuppressReadOnlyValidation();
    }
    
    [Fact]
    public async Task CreateAsync_SingleEntity_ShouldCreateReadOnlyEntity()
    {
        var company = FactoryGirl.Build<TestCompany>();
        var actualCompany = await context.ReadOnlyCompanies.CreateAsync(() => company);

        await context.SaveChangesAsync();

        CompanyEquals(actualCompany, company);

        var savedCompany = await context.ReadOnlyCompanies.FirstOrDefaultAsync(x => x.Id == actualCompany.Id);
        savedCompany.Should().NotBeNull();
        savedCompany.Name.Should().Be(actualCompany.Name);
    }

    [Fact]
    public async Task CreateAsync_ArrayOfEntities_ShouldCreateMultipleReadOnlyEntities()
    {
        // Arrange
        var expectedCompanies = new List<TestCompany> { 
            FactoryGirl.Build<TestCompany>(), 
            FactoryGirl.Build<TestCompany>(),
            FactoryGirl.Build<TestCompany>()
        };

        // Act
        var actualCompanies = await context.ReadOnlyCompanies.CreateAsync(() => expectedCompanies);
        await context.SaveChangesAsync();

        // Assert
        actualCompanies.Should().NotBeNull();
        actualCompanies.Count.Should().Be(3);

        for (var i = 0; i < 3; i++)
            CompanyEquals(actualCompanies[i], expectedCompanies[i]);

        // Verify all companies were saved to database using their actual IDs
        foreach (var company in actualCompanies)
        {
            var savedCompany = await context.ReadOnlyCompanies.FirstOrDefaultAsync(x => x.Id == company.Id);
            savedCompany.Should().NotBeNull();
            savedCompany.Name.Should().Be(company.Name);
        }
    }

    private static void CompanyEquals(TestCompany actual, TestCompany expected)
        => Validate.Begin()
            .IsNotNull(actual, nameof(actual)).Check()
            .IsNotNull(expected, nameof(expected)).Check()
            .IsEqual(expected.Name, actual.Name, nameof(expected.Name))
            .IsEqual(expected.Code, actual.Code, nameof(expected.Code))
            .IsEqual(expected.Address, actual.Address, nameof(expected.Address))
            .IsEqual(expected.IsActive, actual.IsActive, nameof(expected.IsActive))
            .IsEqual(expected.Phone, actual.Phone, nameof(expected.Phone))
            .Check();
    
    public override void Dispose()
    {
        readonlyScope.Dispose();
        context.Dispose();
        base.Dispose();
    }
}