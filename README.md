# Rop.Database.10

Suite of .NET 9 libraries for advanced database operations with Dapper, including change tracking, caching, and repository patterns.

## Projects in this Solution

### Rop.Dapper.ContribEx10
[![NuGet](https://img.shields.io/nuget/v/Rop.Dapper.ContribEx10.svg)](https://www.nuget.org/packages/Rop.Dapper.ContribEx10)
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue.svg)](LICENSE)

Advanced extensions for Dapper and Dapper.Contrib targeting .NET 9.

**Features:**
- Extended CRUD operations with Dapper
- Optimized "Slim" SELECT queries
- Partial key support for composite keys
- Foreign database handling
- Cached SQL query generation
- Full async/await support

[Full Documentation](Rop.Dapper.ContribEx10/Readme.md)

### Rop.AbsDatabase10
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue.svg)](LICENSE)

Abstract database layer providing base functionality for database operations.

**Features:**
- Unit of Work pattern implementation
- Transaction management
- Connection pooling
- Result-oriented error handling with `Rop.Results9`

### Rop.SqlDatabase10
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue.svg)](LICENSE)

SQL Server specific implementation with advanced features.

**Features:**
- SQL Server Change Tracking integration
- Repository pattern implementations
- Cache repository with automatic invalidation
- Type handlers for DateOnly and TimeOnly (.NET 6+)
- Foreign database support

### Rop.CycleTask
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue.svg)](LICENSE)

Background task scheduler for periodic database operations.

**Features:**
- Priority-based task scheduling
- Automatic retry logic
- Task dependency management
- Thread-safe queue implementation

## Quick Start

### Installation

```bash
# Install Dapper extensions
dotnet add package Rop.Dapper.ContribEx10

# Or use internal packages
dotnet add reference ../Rop.SqlDatabase10/Rop.SqlDatabase10.csproj
```

### Basic Usage

```csharp
using Rop.Dapper.ContribEx10;
using Rop.Database10;

// Create database instance
var database = new Database(connectionString);

// Use Dapper extensions
var user = connection.GetSlim<User>(userId);
var users = connection.GetAllSlim<User>();

// Use repository pattern
public class UserRepository : AbsSqlRepository<User>
{
    public UserRepository(Database database) : base(database) { }
}
```

## Architecture

```
Rop.Database.10/
|
+-- Rop.Dapper.ContribEx10/          # Dapper extensions (NuGet package)
|   +-- ConnectionHelper.*.cs        # Extension methods for IDbConnection
|   +-- DapperHelperExtend.*.cs      # Helper utilities
|   +-- Attributes/                  # Custom attributes
|
+-- Rop.AbsDatabase10/               # Abstract database layer
|   +-- AbsDatabase.cs               # Base database class
|   +-- AbsDatabase.*.cs             # Partial implementations
|
+-- Rop.SqlDatabase10/               # SQL Server implementation
|   +-- Database.cs                  # SQL Server database
|   +-- Repository/                  # Repository implementations
|   +-- CacheRepository/             # Cached repository
|   +-- Tracking/                    # Change tracking
|
+-- Rop.CycleTask/                   # Background task scheduler
    +-- CycleTaskService.cs
    +-- CycleQueue.cs
```

## Requirements

- **.NET 9.0** or higher
- **C# 13**
- **SQL Server** (for Rop.SqlDatabase10)
- **Dapper 2.1.66+**
- **Dapper.Contrib 2.0.78+**

### Enable SQL Server Change Tracking

```sql
-- Enable on database
ALTER DATABASE MyDatabase
SET CHANGE_TRACKING = ON
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON);

-- Enable on table
ALTER TABLE MyTable
ENABLE CHANGE_TRACKING
WITH (TRACK_COLUMNS_UPDATED = OFF);
```

## Documentation

- [Rop.Dapper.ContribEx10 Documentation](Rop.Dapper.ContribEx10/Readme.md)

## Testing

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test xUnit.Rop.Dapper.ContribEx10
```

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This entire solution is licensed under **GPL-3.0-or-later**.

All projects in this repository:
- **Rop.Dapper.ContribEx10**
- **Rop.AbsDatabase10**
- **Rop.SqlDatabase10**
- **Rop.CycleTask**

are distributed under the [GNU General Public License v3.0 or later](https://www.gnu.org/licenses/gpl-3.0.html).

See [LICENSE](LICENSE) file for full license text.

### What this means:

- ✅ You can use this software freely
- ✅ You can modify the source code
- ✅ You can distribute modified versions
- ⚠️ Any modifications must also be GPL-3.0-or-later
- ⚠️ You must disclose the source code of derivative works
- ⚠️ You must include copyright and license notices

## Contact

**Ramon Ordiales Plaza**
- GitHub: [@ramoneeza](https://github.com/ramoneeza)

## Acknowledgments

- [Dapper](https://github.com/DapperLib/Dapper) - Micro-ORM for .NET
- [Dapper.Contrib](https://github.com/DapperLib/Dapper.Contrib) - CRUD helpers

## Roadmap

- [x] Basic Dapper extensions
- [x] Repository pattern
- [x] Change tracking integration
- [x] Cache repository
- [ ] Multi-database support (PostgreSQL, MySQL,SQLite)
- [ ] Performance benchmarks

---

**Made with ❤️ by Ramon Ordiales Plaza**

*Copyright © 2025 - All rights reserved*
