# Rop.SqlDatabase10

SQL Server implementation for the Rop.Database10 suite with advanced features including Change Tracking, Repository patterns with DTO support, and automatic cache invalidation.

## Main Features

- **SQL Server Change Tracking Integration**: Monitor database changes and generate events automatically
- **Repository Pattern with DTO Support**: Ready-to-use base classes with direct entity-to-DTO mapping
- **Automatic Cache Invalidation**: Repositories detect table changes from external services and update cache
- **Event-Driven Architecture**: Repositories emit events when data changes are detected via Change Tracking
- **Foreign Database Support**: Handle entities across multiple databases seamlessly
- **Type Handlers**: Support for DateOnly and TimeOnly types (.NET 6+)
- **Unit of Work Pattern**: Inherited from AbsDatabase for transactional operations
- **Transaction Management**: Comprehensive transaction support
- **Full Async/await Support**: All operations have asynchronous versions
- **Compatible with C# 13 and .NET 9**

## Key Concepts

### Change Tracking for Repositories

Unlike traditional background task schedulers, Rop.SqlDatabase10 uses SQL Server Change Tracking to make repositories **reactive to external changes**:

- Repositories subscribe to table changes via Change Tracking
- When data changes from **other services or applications**, repositories:
  - Emit `OnRecordsChanged` events
  - Automatically invalidate affected cache entries
  - Notify dependent components
- No polling or scheduled tasks needed - changes are detected via SQL Server's native mechanism

### DTO-Oriented Design

The library is designed around the **DTO (Data Transfer Object) pattern** with direct mapping on repositories

- **DTO classes** represent data decorated with dapper attributes and property appears as Database exposes
- **Direct mapping methods** Abstract class "map" to provide clear transformation
- Repository methods work with DTOs while persisting final entities internally

## Installation

This is an internal library, typically referenced by project:

```bash
dotnet add reference ../Rop.SqlDatabase10/Rop.SqlDatabase10.csproj
```

Or if published as NuGet package:

```bash
dotnet add package Rop.SqlDatabase10
```

## Dependencies

- **Rop.AbsDatabase10**: Abstract database layer
- **Rop.Dapper.ContribEx10**: Dapper extensions (via AbsDatabase10)
- **Microsoft.Data.SqlClient**: SQL Server data provider
- **System.Runtime.Caching**: Caching infrastructure
- **Rop.Results9**: Result-oriented error handling (via AbsDatabase10)

## Quick Start

### Enable SQL Server Change Tracking

First, enable Change Tracking on your database and tables:

```sql
-- Enable on database
ALTER DATABASE MyDatabase
SET CHANGE_TRACKING = ON
(CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON);

-- Enable on tables you want to monitor
ALTER TABLE Users
ENABLE CHANGE_TRACKING
WITH (TRACK_COLUMNS_UPDATED = OFF);
```

### Basic Database Setup

```csharp
using Rop.Database10;

// Create database instance
var database = new Database(connectionString);

// Use with dependency injection
services.AddSingleton<Database>(sp => 
    new Database(configuration.GetConnectionString("DefaultConnection")));
```

### Cached Repository with Custom Expiration Time

```csharp
using Rop.Database10.CacheRepository;

public class CachedUserRepository : AbsCacheSqlDtoRepositoryK<User, UserDto, int>
{
    public CachedUserRepository(Database database) 
        : base(
            database, 
            useslim: true,                                    // Use optimized slim queries
            defaultExpirationTime: TimeSpan.FromMinutes(10),  // Expire after 10 minutes
            changespriority: 3)                               // Change tracking priority
    { }
    
    protected override User Map(UserDto dto)
    {
        return new User { Id = dto.Id, Name = dto.Name, Email = dto.Email };
    }
}

// Repository behavior:
var cachedRepo = new CachedUserRepository(database);

// First call at T=0 - hits database and caches
var user = await cachedRepo.Get(userId);

// Second call at T=5 - returns from cache (still valid)
var userAgain = await cachedRepo.Get(userId);

// Third call at T=11 - cache expired, reloads from database
var userExpired = await cachedRepo.Get(userId);

// If another service updates the Users table at any time:
// - Change Tracking immediately expires the affected cache entry
// - Next Get(userId) will fetch fresh data regardless of time
```

## Repository Base Classes

### Standard Repositories (with Change Tracking)

These repositories monitor table changes via SQL Server Change Tracking and automatically reload data:

- **`AbsSqlRepository<T>`**: Repository with string key, no DTO separation (T=Entity=DTO)
- **`AbsSqlIntRepository<T>`**: Repository with int key, no DTO separation (T=Entity=DTO)
- **`AbsSqlRepositoryK<T, D, K>`**: Generic repository with DTO mapping
  - `T`: Entity type (your business model)
  - `D`: DTO type (table representation with Dapper attributes)
  - `K`: Key type (string, int, or custom)

### DTO Repositories (Recommended)

Specialized versions with DTO support and automatic Change Tracking:

- **`AbsSqlDtoRepository<T, D>`**: DTO repository with string key
- **`AbsSqlDtoIntRepository<T, D>`**: DTO repository with int key
  - Use when you need to separate database schema (DTO) from business logic (Entity)
  - Implement `Map(D item)` to convert DTO → Entity

### Simple Repositories (without Change Tracking)

Manual reload control, no automatic change detection:

- **`AbsSimpleSqlRepository<T, D>`**: Simple repository with string key
- **`AbsSimpleSqlIntRepository<T, D>`**: Simple repository with int key
- **`AbsSimpleSqlRepositoryK<T, D, K>`**: Generic simple repository with custom key
  - Lower overhead, suitable for static data or single-instance applications
  - Call `Reset()` or `ResetIds()` manually when data changes

### Partial Key Repositories

For entities with composite keys (e.g., OrderDetails with OrderId + ProductId):

- **`AbsSimpleSqlPartialRepositoryK<T, K>`**: Repository for entities with PartialKey attributes
  - Supports `GetPartial(key1)` to get all records matching first key
  - Supports `InsertOrUpdatePartial(key1, items)` to replace all records with same key1

### Cached Repositories (with Auto-Invalidation and Time-Based Expiration)

Memory-cached repositories with **automatic time-based expiration** and Change Tracking invalidation:

- **`AbsCacheSqlRepository<T, K>`**: Cached repository without DTO separation
- **`AbsCacheSqlRepositoryK<T, D, K>`**: Cached repository with DTO mapping
- **`AbsCacheSqlDtoRepositoryK<T, D, K>`**: Specialized cached DTO repository
- **`AbsSimpleCacheSqlRepositoryK<T, D, K>`**: Simple cached repository (manual invalidation)

**Key Features:**
- **Configurable expiration time** (`DefaultExpirationTime`) - default 5 minutes
- **Automatic cleanup** of expired entries (`DefaultExpirationCleanTime`) - default 10 minutes
- **Dual invalidation strategy**:
  - Time-based: Entries expire after `DefaultExpirationTime`
  - Event-based: Change Tracking immediately expires affected cache entries
- Items are reloaded from database automatically when accessed after expiration

### Repository Comparison

| Repository Type | Change Tracking | Time-Based Cache | DTO Support | Use Case |
|----------------|-----------------|------------------|-------------|----------|
| `AbsSqlRepository<T>` | ✅ Yes | ❌ No | ❌ No | Standard CRUD with auto-reload |
| `AbsSqlDtoRepository<T,D>` | ✅ Yes | ❌ No | ✅ Yes | API responses, data transformation |
| `AbsSimpleSqlRepository<T,D>` | ❌ No | ❌ No | ✅ Yes | Static data, single instance |
| `AbsCacheSqlRepository<T,K>` | ✅ Yes | ✅ Yes (5 min) | ❌ No | High-read, low-write data |
| `AbsCacheSqlDtoRepositoryK<T,D,K>` | ✅ Yes | ✅ Yes (5 min) | ✅ Yes | Cached API responses |

**Note:** Time-based cache default is 5 minutes expiration, configurable per repository.

## When to Use Change Tracking

✅ **Use Change Tracking when:**
- Multiple services/applications access the same database
- You need to react to external data changes
- You want automatic cache invalidation

## Cache Strategy: Dual Invalidation

Cached repositories use a **dual invalidation strategy** to ensure data freshness:

### 1. Time-Based Expiration (Configurable)

```csharp
// Configure expiration time per repository
public class CachedProductRepository : AbsCacheSqlRepositoryK<Product, int>
{
    public CachedProductRepository(Database database) 
        : base(
            database,
            defaultExpirationTime: TimeSpan.FromMinutes(15),  // Products expire after 15 min
            defaultcleantime: TimeSpan.FromMinutes(30))       // Cleanup every 30 min
    { }
}
```

**How it works:**
- Each cached item has a timestamp
- When accessed, the repository checks if `(now - timestamp) > expirationTime`
- If expired, item is automatically reloaded from database
- Background cleanup removes expired items periodically

**Use when:**
- Data changes infrequently
- You can tolerate some staleness (seconds/minutes)
- You want predictable cache refresh

### 2. Event-Based Invalidation (Change Tracking)

```csharp
// Change Tracking automatically invalidates affected items
public class CachedUserRepository : AbsCacheSqlDtoIntRepository<User, UserDto>
{
    public CachedUserRepository(Database database) 
        : base(
            database,
            defaultExpirationTime: TimeSpan.FromHours(1),  // Long expiration
            changespriority: 1)                            // High priority for changes
    { }
    
    protected override User Map(UserDto dto) => /* ... */;
}
```

**How it works:**
- SQL Server tracks changes in `CHANGETABLE()`
- `SqlTableDependency` polls for changes every few seconds
- When changes detected, affected cache entries are **immediately expired**
- Next access reloads fresh data from database

**Use when:**
- Multiple services modify the same tables
- You need near-real-time consistency
- External changes must be reflected immediately

### 3. Combined Strategy

```csharp
// Best of both worlds: long time expiration + immediate invalidation
public class OptimalCachedRepository : AbsCacheSqlRepositoryK<Entity, int>
{
    public OptimalCachedRepository(Database database) 
        : base(
            database,
            defaultExpirationTime: TimeSpan.FromHours(4),   // Long expiration for stability
            changespriority: 3)                             // Medium priority
    { }
}
```

**Benefits:**
- ✅ Fast reads from cache (hours of validity)
- ✅ Immediate updates when data changes externally
- ✅ Automatic fallback if Change Tracking fails
- ✅ Reduced database load

**Example timeline:**
```
T=0:00    User calls Get(1) → Cache miss → Load from DB → Cache [valid until T=4:00]
T=0:30    User calls Get(1) → Cache hit → Return cached value
T=1:15    External service updates User(1) → Change Tracking fires → Cache EXPIRED
T=1:16    User calls Get(1) → Cache expired → Reload from DB → Cache [valid until T=5:16]
T=2:00    User calls Get(1) → Cache hit → Return cached value
T=5:17    User calls Get(1) → Time expired → Reload from DB → Cache [valid until T=9:17]
```

### Performance Impact

**Time-based expiration:**
- ✅ Very low overhead (timestamp comparison)
- ✅ No database queries until expiration
- ✅ Predictable memory usage

**Change Tracking:**
- ⚠️ Small overhead per change detection
- ⚠️ Requires SQL Server Change Tracking enabled
- ✅ Immediate invalidation on changes

**Combined:**
- ✅ Best read performance (long cache lifetime)
- ✅ Best consistency (immediate invalidation)
- ⚠️ Requires SQL Server Change Tracking enabled

## License

This project is licensed under **GPL-3.0-or-later**.

See [LICENSE](../LICENSE) file for details.

## Links

- **GitHub Repository**: [https://github.com/ramoneeza/Rop.Database.10](https://github.com/ramoneeza/Rop.Database.10)
- **Main README**: [Solution Overview](../README.md)
- **Rop.Dapper.ContribEx10**: [Full Documentation](../Rop.Dapper.ContribEx10/Readme.md)

## Related Projects

- **Rop.Dapper.ContribEx10**: Dapper extensions
- **Rop.AbsDatabase10**: Abstract database layer
- **Rop.Results9**: Result-oriented error handling

---

*Copyright © 2025 Ramon Ordiales Plaza - Licensed under GPL-3.0-or-later*
