using AwesomeAssertions;
using Bounteous.Data.Extensions.Readonly;
using Bounteous.Data.Extensions.Tests.Context;
using Bounteous.Data.Extensions.Tests.Helpers;
using Bounteous.xUnit.Accelerator.Factory;

namespace Bounteous.Data.Extensions.Tests.Domain;

[Collection("Bounteous")]
public class ReadOnlyDbSetValidationTests : DbContextTestBase
{
    private readonly TestDbContext context;
    private readonly IDisposable readonlyScope;

    public ReadOnlyDbSetValidationTests()
    {
        context = CreateContext();
        readonlyScope = context.SuppressReadOnlyValidation();
    }

    [Fact]
    public async Task CreateAsyncShouldThrowWhenFactoryIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(async () => 
            await context.ReadOnlyCompanies.CreateAsync((Func<TestCompany>)null!));
    }

    [Fact]
    public void AsReadOnly_ShouldCreateReadOnlyWrapper()
    {
        var dbSet = context.Companies;
        var readOnlyDbSet = dbSet.AsReadOnly<TestCompany, long>();
        readOnlyDbSet.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsyncShouldPreserveEntityProperties()
    {
        // Arrange
        var originalCompany =  FactoryGirl.Build<TestCompany>();

        // Act
        var createdCompany = await context.ReadOnlyCompanies.CreateAsync(() => originalCompany);
        await context.SaveChangesAsync();

        // Assert
        createdCompany.Should().NotBeNull();
        createdCompany.Name.Should().Be(originalCompany.Name);
        createdCompany.Code.Should().Be(originalCompany.Code);
        createdCompany.Address.Should().Be(originalCompany.Address);
        createdCompany.Phone.Should().Be(originalCompany.Phone);
        createdCompany.Email.Should().Be(originalCompany.Email);
        createdCompany.Website.Should().Be(originalCompany.Website);
        createdCompany.IsActive.Should().Be(originalCompany.IsActive);
    }

    [Fact]
    public async Task CreateAsyncEmptyListShouldReturnEmptyList()
    {
        var emptyList = new List<TestCompany>();
        var result = await context.ReadOnlyCompanies.CreateAsync(() => emptyList);
        result.Should().NotBeNull();
        result.Count.Should().Be(0);
    }

    public override void Dispose()
    {
        readonlyScope.Dispose();
        context.Dispose();
        base.Dispose();
    }
}
