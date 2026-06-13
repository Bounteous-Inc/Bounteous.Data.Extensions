# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

Development-only EF Core utility library. Provides extension methods on `ReadOnlyDbSet<T>` that bypass read-only validation for test seeding and data migrations. Guards against production use via multi-strategy environment detection.

## Commands

```bash
# Build
dotnet build Bounteous.Data.Extensions.sln
dotnet build Bounteous.Data.Extensions.sln --configuration Release

# Test (all)
dotnet test src/Bounteous.Data.Extensions.Tests/Bounteous.Data.Extensions.Tests.csproj

# Test (single test by name)
dotnet test src/Bounteous.Data.Extensions.Tests/Bounteous.Data.Extensions.Tests.csproj \
  --filter "FullyQualifiedName~<TestMethodName>"

# Test (single class)
dotnet test src/Bounteous.Data.Extensions.Tests/Bounteous.Data.Extensions.Tests.csproj \
  --filter "ClassName=Bounteous.Data.Extensions.Tests.Domain.<ClassName>"
```

## Architecture

### Production Safety Chain

Every extension method call flows through this chain before executing:

```
ReadOnlyDbSetExtensions.CreateAsync()
  → ProductionWarningMarker.ValidateContext()
    → ProductionDetector.IsProductionEnvironment (Lazy<bool>)
    → ProductionDetector.IsAllowedContext (Lazy<bool>)
```

**`ProductionDetector`** (`Utilities/ProductionDetector.cs`) — lazy-cached, multi-strategy detection:
- Always returns `IsProductionEnvironment = false` in `DEBUG` builds
- In `RELEASE`: checks env vars (`ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT`, cloud provider vars), process names (IIS, nginx), and assembly name patterns before defaulting to production=true
- `IsAllowedContext`: detects test/migration context via presence of test framework assemblies (xunit, nunit, moq, etc.), EF tooling process names (`dotnet-ef`), call stack analysis

**`ProductionWarningMarker`** (`ProductionWarningMarker.cs`) — throws `InvalidOperationException` if production is detected and context is not allowed.

**`ReadOnlyDbSetExtensions`** (`Readonly/ReadOnlyDbSetExtensions.cs`) — the public API:
- `CreateAsync<T, TKey>(Func<T> entityFactory)` — creates single entity, bypasses read-only via reflection
- `CreateAsync<T, TKey>(Func<List<T>> entityFactory)` — bulk variant, returns `List<T>`
- Generic constraints: `T : class, IReadOnlyEntity<TKey>`

### Dependencies

- **`Bounteous.Data`** — provides `DbContextBase<TKey>`, `ReadOnlyDbSet<T>`, `IReadOnlyEntity<TKey>`
- **`Bounteous.Core`** — validation utilities, type helpers
- **`Microsoft.EntityFrameworkCore 10.0.9`** — `DbSet<T>` accessed via reflection inside ReadOnlyDbSet

### Test Infrastructure

Tests use xUnit + AwesomeAssertions + Moq. The in-memory EF provider (`Microsoft.EntityFrameworkCore.InMemory`) backs `TestDbContext`. `Bounteous.xUnit.Accelerator` provides the FactoryGirl-style factory pattern (`CleanFactory`, `EntityFactory`).

## CI/CD

- **CI build** — triggers on push to any non-`main` branch; builds + tests via `bounteous-dotnet-common-workflows/ci-build.yml@main`
- **Publish** — triggers on PR merged to `main`; uses NuGet Trusted Publishing (OIDC, no API key secret) via `bounteous-dotnet-common-workflows/build-and-publish-to-nuget.yml@main`; auto-increments patch version and commits updated `<Version>` back to `main` with `[skip ci]`

Version is managed by CI — do not manually edit `<Version>` in the `.csproj`.
