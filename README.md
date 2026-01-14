# Bounteous.Data.Extensions

⚠️ **DEVELOPMENT ONLY** - Developer utilities and extensions for Bounteous.Data

## Overview

Bounteous.Data.Extensions provides developer utilities for Bounteous.Data including extensions for creating test objects and bypassing read-only validation in controlled environments.

## ⚠️ Important Warning

**NOT INTENDED FOR PRODUCTION USE**

This package bypasses read-only validation and should only be used in:
- Unit test projects
- Data migration projects  
- Development environments

**DO NOT USE IN PRODUCTION CODE.**

## Key Features

- ✅ **ReadOnlyDbSetExtensions** - Utilities for creating test objects with read-only entities
- ✅ **Development Helpers** - Bypass validation for testing and migration scenarios
- ✅ **Test Object Creation** - Simplified creation of entities for unit testing

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

### Test Object Creation

```csharp
using Bounteous.Data.Extensions;

// Create test entities bypassing read-only validation
var testEntity = readOnlyDbSet.CreateTestObject();
```

### Development Scenarios

```csharp
// For data migrations or development setup
using var scope = readOnlyDbSet.BypassReadOnlyValidation();
// Perform operations that would normally be blocked
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Repository

https://github.com/Bounteous-Inc/Bounteous.Data.Extensions
