# Bounteous.Data.Extensions

⚠️ **NOT INTENDED FOR PRODUCTION USE** ⚠️

Developer utilities for Bounteous.Data including ReadOnlyDbSetExtensions for creating test objects in unit tests and data migrations.

## Overview

`Bounteous.Data.Extensions` provides specialized extension methods that enable the creation and modification of read-only entities in testing scenarios. This package is designed specifically for:

- **Unit Tests**: Creating test data for read-only entities
- **Data Migrations**: Seeding read-only entities during database initialization
- **Integration Tests**: Setting up test scenarios with read-only data

## ⚠️ PRODUCTION WARNING

**THIS PACKAGE IS NOT INTENDED FOR PRODUCTION USE**

This package bypasses read-only validation mechanisms and should only be used in:
- Unit test projects
- Data migration projects  
- Development/Integration test environments

**DO NOT REFERENCE THIS PACKAGE IN PRODUCTION CODE**
## Features

- **ReadOnlyDbSetExtensions**: Extension methods for `ReadOnlyDbSet<T, TKey>` that bypass read-only validation
- **Multiple Creation Patterns**: Support for single entities, arrays, lists, and collections
- **Type Safety**: Full generic type support with compile-time checking
- **DbContext Extensions**: Context-level validation suppression for test scenarios

## Installation

```bash
dotnet add package Bounteous.Data.Extensions
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data.Extensions
```

## Dependencies

- .NET 10.0+
- Bounteous.Data
- Bounteous.Core
- Microsoft.EntityFrameworkCore

## Usage

### Basic Test Setup

The most common use case is in unit tests where you need to create read-only entities:

```csharp
using Bounteous.Data.Extensions;
using Bounteous.Data.Extensions.Readonly;

[Collection("ApplicationTests")]
public class ReadOnlyEntityTests : IDisposable
{
    private readonly TestDbContext context;
    private readonly IDisposable readonlyScope;

    public ReadOnlyEntityTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var identityProvider = new TestCurrentUserService();
        context = new TestDbContext(options, null, identityProvider);
        
        // Suppress read-only validation for testing
        readonlyScope = context.SuppressReadOnlyValidation();
    }

    [Fact]
    public async Task CanCreateReadOnlyEntity()
    {
        // Arrange & Act
        var project = await context.Projects.CreateAsync(() => new Project
        {
            Name = "Test Project",
            Code = "TEST001"
        });

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
    }

    public void Dispose()
    {
        readonlyScope.Dispose();
        context.Dispose();
    }
}
```

### Creating Multiple Entities

```csharp
[Fact]
public async Task CanCreateMultipleReadOnlyEntities()
{
    // Create an array of entities
    var companies = await context.Companies.CreateAsync(() => new[]
    {
        new Company { Name = "Company A", Code = "COMPA" },
        new Company { Name = "Company B", Code = "COMPB" },
        new Company { Name = "Company C", Code = "COMPC" }
    });

    Assert.Equal(3, companies.Length);
    Assert.All(companies, c => Assert.NotNull(c.Id));

    // Create a list of entities
    var projects = await context.Projects.CreateAsync(() => new List<Project>
    {
        new Project { Name = "Project 1", CompanyId = companies[0].Id },
        new Project { Name = "Project 2", CompanyId = companies[1].Id }
    });

    Assert.Equal(2, projects.Count);
}
```

### Data Migration Example

```csharp
public class TestDataMigration : Migration
{
    private readonly MyDbContext context;
    private readonly IDisposable readonlyScope;

    public TestDataMigration(MyDbContext context)
    {
        this.context = context;
        this.readonlyScope = context.SuppressReadOnlyValidation();
    }

    public async Task SeedAsync()
    {
        // Use ReadOnlyDbSetExtensions to seed read-only entities
        await context.Companies.CreateAsync(() => new[]
        {
            new Company
            {
                Name = "Bounteous Inc.",
                Code = "BOU001",
                Address = "123 Business Park Dr, Suite 100, Austin, TX 78759",
                Phone = "512-555-0123",
                Email = "info@bounteous.com",
                Website = "www.bounteous.com",
                IsActive = true
            },
            new Company
            {
                Name = "Demo Construction Co",
                Code = "DEMO01",
                Address = "456 Construction Ave, Dallas, TX 75201",
                Phone = "214-555-0456",
                Email = "contact@democonstruction.com",
                Website = "www.democonstruction.com",
                IsActive = true
            }
        });

        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        readonlyScope.Dispose();
    }
}
```

### Integration Test Example

```csharp
public class ReadOnlyEntityIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;
    private readonly IDisposable readonlyScope;

    public ReadOnlyEntityIntegrationTests(TestFixture fixture)
    {
        this.fixture = fixture;
        this.readonlyScope = fixture.Context.SuppressReadOnlyValidation();
    }

    [Fact]
    public async Task CanPerformComplexOperationsWithReadOnlyEntities()
    {
        var context = fixture.Context;

        // Create read-only reference data
        var statuses = await context.Statuses.CreateAsync(() => new[]
        {
            new Status { Name = "Active", IsDefault = true },
            new Status { Name = "Inactive", IsDefault = false },
            new Status { Name = "Pending", IsDefault = false }
        });

        // Create entities that reference the read-only data
        var users = await context.Users.CreateAsync(() => new[]
        {
            new User 
            { 
                Name = "John Doe", 
                Email = "john@example.com",
                StatusId = statuses.First(s => s.Name == "Active").Id
            },
            new User 
            { 
                Name = "Jane Smith", 
                Email = "jane@example.com",
                StatusId = statuses.First(s => s.Name == "Pending").Id
            }
        });

        await context.SaveChangesAsync();

        // Verify the data was created correctly
        var activeUsers = await context.Users
            .Where(u => u.Status.Name == "Active")
            .ToListAsync();

        Assert.Single(activeUsers);
        Assert.Equal("John Doe", activeUsers.First().Name);
    }

    public void Dispose()
    {
        readonlyScope.Dispose();
    }
}
```

## API Reference

### DbContext Extensions

#### `SuppressReadOnlyValidation()`
Creates a scope that suppresses read-only validation for all operations within the scope.

```csharp
using var scope = context.SuppressReadOnlyValidation();
// All read-only validation is suppressed within this scope

Creates a list of entities using the provided factory function.

**Parameters:**
- `readOnlyDbSet`: The ReadOnlyDbSet to add the entities to
- `entityFactory`: A factory function that creates a list of entities

**Returns:** The list of created entities

**⚠️ Production Warning:** This method bypasses read-only validation and will throw in production environments.

## Dependencies

- .NET 10.0
- Microsoft.EntityFrameworkCore 9.0.0+
- Bounteous.Data 1.0.0+

## Installation

```bash
dotnet add package Bounteous.Data.Extensions
```

## Important Notes

⚠️ **This package is intended for testing and migration scenarios only.**  
The extension methods bypass read-only validation and should not be used in production application code.

- **Use in Unit Tests**: Perfect for creating test data
- **Use in Data Migrations**: Ideal for seeding read-only entities
- **Do NOT use in Production**: Avoid using in regular application code

## Production Protection

This package includes multiple layers of protection against production usage:

1. **Runtime Checks**: Throws `InvalidOperationException` if used in production environment
2. **Documentation Warnings**: Clear warnings in all XML documentation
3. **Package Metadata**: Warning indicators in package title and description
4. **Environment Detection**: Automatically detects production environments via environment variables

## License

Copyright © Bounteous Inc. 2026. Licensed under MIT License.
