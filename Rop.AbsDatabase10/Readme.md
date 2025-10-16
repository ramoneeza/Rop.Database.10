# Rop.AbsDatabase10

Abstract database layer providing base functionality for database operations with Unit of Work pattern, transaction management, and result-oriented error handling.

## Main Features

- **Abstract Base Class**: Foundation for database implementations (SQL Server, PostgreSQL, etc.)
- **Unit of Work Pattern**: Execute multiple operations in a single transaction
- **Connection Management**: Automatic connection lifecycle handling
- **Result-Oriented Error Handling**: Integration with `Rop.Results9` for robust error handling
- **Transaction Support**: Comprehensive transaction management with automatic rollback
- **Extension Methods**: Helper methods for common database operations
- **Connection String Parsing**: Automatic extraction of server and database name
- **Full Async/await Support**: All operations have asynchronous versions
- **Compatible with C# 13 and .NET 9**

## Overview

`Rop.AbsDatabase10` is the abstract foundation layer that provides:
- Base infrastructure for database operations
- Unit of Work pattern implementation  
- Connection and transaction management
- Integration point for Dapper and Dapper.Contrib extensions

This library is extended by concrete implementations like `Rop.SqlDatabase10`.

## Installation

This is an internal library, typically referenced by other projects:

```bash
dotnet add reference ../Rop.AbsDatabase10/Rop.AbsDatabase10.csproj
```

## Dependencies

- **Rop.Dapper.ContribEx10**: Dapper extensions for CRUD operations
- **Dapper.Contrib**: CRUD helpers for Dapper  
- **Rop.Results9**: Result-oriented error handling library

## Quick Start

### Implementing a Concrete Database

```csharp
using Rop.Database;

public class MyDatabase : AbsDatabase
{
    public MyDatabase(string connectionString) : base(connectionString) { }

    // Implement abstract factory method
    public override DbConnection FactoryConnection()
    {
        return new SqlConnection(Strconn);
    }
}
```

### Using Unit of Work

```csharp
var database = new MyDatabase(connectionString);

// Execute in transaction - return value
var result = database.UnitOfWork<User>(conn =>
{
    var user = conn.Get<User>(userId);
    user.LastLogin = DateTime.Now;
    conn.Update(user);
    return user;
});

// Execute in transaction - void
var voidResult = database.UnitOfWorkVoid(conn =>
{
    conn.Delete<User>(userId);
    conn.Insert(new User { Name = "New User" });
});
```

### Async Unit of Work

```csharp
// Async with return value
var user = await database.UnitOfWorkAsync<User>(async conn =>
{
    var user = await conn.GetAsync<User>(userId);
    user.LastLogin = DateTime.Now;
    await conn.UpdateAsync(user);
    return user;
});
```

### Result-Oriented Error Handling

```csharp
// Check for success
var result = database.Get<User>(userId);
if (result.IsSuccess)
{
    var user = result.Value;
    Console.WriteLine($"User: {user.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}

// Throw on failure
var user = database.Get<User>(userId)
    .ThrowIfFailed()
    .Value;
```

## License

This project is licensed under **GPL-3.0-or-later**.

See [LICENSE](../LICENSE) file for details.

## Links

- **GitHub Repository**: [https://github.com/ramoneeza/Rop.Database.10](https://github.com/ramoneeza/Rop.Database.10)
- **Main README**: [Solution Overview](../README.md)
- **Rop.SqlDatabase10**: [SQL Server Implementation](../Rop.SqlDatabase10/Readme.md)
- **Rop.Dapper.ContribEx10**: [Dapper Extensions](../Rop.Dapper.ContribEx10/Readme.md)

## Related Projects

- **Rop.SqlDatabase10**: SQL Server concrete implementation
- **Rop.Dapper.ContribEx10**: Dapper extensions
- **Rop.Results9**: Result-oriented error handling

## Documentation

- [Rop.Dapper.ContribEx10 Documentation] - Dapper extensions and helpers
- [Rop.AbsDatabase10 Documentation] - Abstract database layer and Unit of Work  ✅ NUEVO
- [Rop.SqlDatabase10 Documentation] - Repository patterns and Change Tracking

---

*Copyright © 2025 Ramon Ordiales Plaza - Licensed under GPL-3.0-or-later*
