# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Purpose

`Bounteous.Data.Extensions` is a **development/test-only** NuGet package that extends `Bounteous.Data` to allow creating entities in read-only DbSets — bypassing read-only validation via reflection. **Never intended for production use.** Guards against accidental production use via `ProductionWarningMarker` / `ProductionDetector`.

## Commands

```bash
dotnet build                                               # build solution
dotnet test                                                # run all tests
dotnet test --filter "DisplayName~SomeTestName"           # run single test
dotnet test --filter "ClassName~ReadOnlyDbSetExtensions"  # run test class
```

CI publishes to NuGet automatically on merge to `main` via trusted publishing (`.github/workflows/build-and-publish.yml`). PRs run build-only CI (`.github/workflows/ci-build.yml`).

## Architecture

Two projects in `src/`:
- `Bounteous.Data.Extensions` — the library
- `Bounteous.Data.Extensions.Tests` — xUnit tests

### Library internals

**`ReadOnlyDbSetExtensions`** (`Readonly/`) — the core feature. Extension methods on `ReadOnlyDbSet<T, TKey>` that use reflection to access the private `InnerDbSet` property and add entities directly. Entry point for all callers.

**`ProductionWarningMarker`** — validation gate called at the top of every extension method. Throws `InvalidOperationException` if running in production.

**`ProductionDetector`** (`Utilities/`) — lazy-cached, multi-strategy detection: DEBUG build flag → explicit env vars (`ASPNETCORE_ENVIRONMENT` etc.) → cloud hosting indicators → assembly name patterns → migration context → test framework assemblies. Results cached via `Lazy<bool>`.

**`[ProductionUsage]`** (`Attributes/`) — attribute to mark APIs unsafe for production. Intended for static analysis warnings.

### Test patterns

All tests share `[Collection("Bounteous")]` backed by `TestCollection` / `TestFixture` (xUnit collection fixture). `TestFixture.ctor` chains `CleanFactory → EntityFractory` (note: filename typo intentional) to seed data; `Dispose` runs `CleanFactory` again.

`DbContextTestBase` (abstract) owns `DbContextOptions`, `MockObserver`, and `IdentityProvider` — extend for any test needing a real `TestDbContext`.

Test assertions use `AwesomeAssertions` (`Validate.Begin()...Check()` fluent API) — not FluentAssertions.

`TestCompany : ReadOnlyEntityBase<long>` is the canonical test entity; `TestDbContext : DbContextBase<long>` uses an in-memory EF Core provider.
