# Rop.Dapper.ContribEx10

Advanced extensions for Dapper, enabling operations such as Delete, Get, Insert, Update, and Join on database connections in .NET 9.

## Main Features

- **Extended CRUD operations**: Enhanced methods for Create, Read, Update, and Delete operations
- **Optimized queries**: Slim SELECT queries that only retrieve necessary columns
- **Partial key support**: Work with composite keys through the `PartialKey` attribute
- **Foreign database support**: Handle entities from different databases
- **Cached SQL generation**: Pre-compiled and cached SQL queries for optimal performance
- **Async/await support**: Full asynchronous API for all operations
- **Partial class organization**: Clean code separation in multiple files
- **Compatible with C# 13 and .NET 9**

## Installation

Add the NuGet package to your project:

```bash
dotnet add package Rop.Dapper.ContribEx10
```

Or via Package Manager Console:

```powershell
Install-Package Rop.Dapper.ContribEx10
```

## Project Structure

- **`ConnectionHelper`**: Extension methods for `IDbConnection` distributed across partial files:
  - `ConnectionHelper.Delete.cs` - Delete operations
  - `ConnectionHelper.GetSlim.cs` - Optimized SELECT queries
  - `ConnectionHelper.GetSome.cs` - Batch retrieval operations
  - `ConnectionHelper.InsertOrUpdate.cs` - Insert and update operations
  - `ConnectionHelper.Join.cs` - JOIN queries and partial key operations

- **`DapperHelperExtend`**: Helper class to access internal Dapper information:
  - `DapperHelperExtend.cs` - Core helper methods
  - `DapperHelperExtend.KeyData.cs` - Key description methods
  - `DapperHelperExtend.PartialKeyData.cs` - Partial key methods
  - `DapperHelperExtend.InternalData.cs` - Internal cache access

- **Key Description Classes**:
  - `KeyDescription` - Immutable class describing entity keys
  - `PartialKeyDescription` - Describes composite partial keys
  - `IKeyDescription` - Interface for key descriptions

- **Attributes**:
  - `ForeignDatabaseAttribute` - Mark entities from foreign databases
  - `PartialKeyAttribute` - Mark properties as partial keys

## License

This project is distributed under the **GPL-3.0-or-later** license.

## XML Documentation

The project generates XML documentation at build time:

```
bin\<Configuration>\<TargetFramework>\Rop.Dapper.ContribEx10.xml
```

This file contains `<summary>`, `<param>`, and `<returns>` comments for IntelliSense support.

---

## API Reference

### DapperHelperExtend

Static helper class providing access to internal Dapper information and SQL query generation.

#### Internal Cache Access Methods

Access internal Dapper reflection caches:

```csharp
List<PropertyInfo> ExplicitKeyPropertiesCache(Type type);
List<PropertyInfo> KeyPropertiesCache(Type type);
List<PropertyInfo> TypePropertiesCache(Type type);
List<PropertyInfo> ComputedPropertiesCache(Type type);
ISqlAdapter GetFormatter(IDbConnection connection);
```

#### Table and Column Methods

```csharp
string GetTableName(Type type);
string GetForeignDatabaseName(Type type);
IEnumerable<string> GetColumnNames(IEnumerable<PropertyInfo> props);
IEnumerable<string> GetColumnNames(Type type, bool excludeautoKey);
```

#### Key Information Methods

Retrieve key metadata from entity types:

```csharp
// Single key methods
(PropertyInfo propkey, bool isautokey) GetSingleKey(Type t);
bool TryGetKey(Type t, out PropertyInfo? propkey, out PropertyInfo? propkey2, 
               out bool isautokey, out bool ispartial);

// Key description methods
KeyDescription? GetAnyKeyDescription(Type t);
KeyDescription GetKeyDescription(Type t);
KeyDescription GetKeyDescription<T>() where T : class;

// Key value methods
object GetKeyValue<T>(T item);
void SetKeyValue<T>(T item, object value);
(KeyDescription keydescription, object value) GetKeyDescriptionAndValue<T>(T item) where T : class;
```

#### Partial Key Methods

Work with composite partial keys:

```csharp
PartialKeyDescription GetPartialKeyDescription(Type t);
PartialKeyDescription GetPartialKeyDescription<T>() where T : class;
object? GetPartialKeyValue<T>(T item) where T : class;
object? GetPartialKey2Value<T>(T item) where T : class;
(PartialKeyDescription keydescription, object key) GetPartialKeyDescriptionAndValue<T>(T item) where T : class;
```

#### Cached SQL Query Generators

Generate and cache SQL statements for optimal performance:

```csharp
// SELECT queries
string SelectGetCache(Type type);
string SelectGetAllCache(Type type);
string SelectGetSlimCache(Type type);
string SelectGetAllSlimCache(Type type);
string SelectGetPartialCache(Type type);
string SelectGetPartial2Cache(Type type);
string SelectGetPartial12Cache(Type type);

// DELETE queries
string DeleteByKeyCache(Type type);
string DeleteByPartialKeyCache(Type type);

// INSERT/UPDATE queries
string InsertNoKeyAttCache(Type type);
string InsertOrUpdateMergeCache(Type type);
```

#### Key List Formatting

Convert collections of keys to SQL-compatible strings:

```csharp
string GetIdList(IEnumerable<int> ids);
string GetIdList(IEnumerable<string> ids);
string GetIdListDyn(IEnumerable ids);
```

#### Expression Helpers

```csharp
string GetMemberName<T>(this Expression<T> expression);
```

---

### ConnectionHelper

Extension methods for `IDbConnection` to perform database operations.

#### Delete Methods

Delete entities by primary key or partial key.

**Synchronous:**

```csharp
bool DeleteByKey<T>(this IDbConnection conn, dynamic id, 
                    IDbTransaction? tr = null, int? commandTimeout = null);

int DeleteByPartialKey<T>(this IDbConnection conn, dynamic id, 
                          IDbTransaction? tr = null, int? commandTimeout = null);
```

**Asynchronous:**

```csharp
Task<bool> DeleteByKeyAsync<T>(this IDbConnection conn, dynamic id, 
                               IDbTransaction? tr = null, int? commandTimeout = null);

Task<int> DeleteByPartialKeyAsync<T>(this IDbConnection conn, dynamic id, 
                                     IDbTransaction? tr = null, int? commandTimeout = null);
```

#### GetSlim Methods

Optimized SELECT queries retrieving only non-computed columns.

**Synchronous:**

```csharp
T? GetSlim<T>(this IDbConnection connection, dynamic id, 
              IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

IEnumerable<T> GetAllSlim<T>(this IDbConnection connection, 
                              IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

List<T> GetSomeSlim<T>(this IDbConnection conn, IEnumerable ids, 
                       IDbTransaction? tr = null) where T : class;

List<T> GetWhereSlim<T>(this IDbConnection conn, string where, object? param = null, 
                        IDbTransaction? tr = null) where T : class;
```

**Asynchronous:**

```csharp
Task<T?> GetSlimAsync<T>(this IDbConnection connection, dynamic id, 
                         IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<IEnumerable<T>> GetAllSlimAsync<T>(this IDbConnection connection, 
                                        IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<IEnumerable<T>> GetSomeSlimAsync<T>(this IDbConnection conn, IEnumerable ids, 
                                         IDbTransaction? tr = null) where T : class;

Task<IEnumerable<T>> GetWhereSlimAsync<T>(this IDbConnection conn, string where, object? param = null, 
                                          IDbTransaction? tr = null) where T : class;
```

#### GetSome Methods

Standard SELECT queries for multiple entities.

**Synchronous:**

```csharp
IEnumerable<T> GetSome<T>(this IDbConnection conn, IEnumerable ids, 
                          IDbTransaction? tr = null, int? commandTimeout = null) where T : class;

IEnumerable<T> GetWhere<T>(this IDbConnection conn, string where, object? param = null, 
                           IDbTransaction? tr = null, int? commandTimeout = null) where T : class;
```

**Asynchronous:**

```csharp
Task<IEnumerable<T>> GetSomeAsync<T>(this IDbConnection conn, IEnumerable ids, 
                                     IDbTransaction? tr = null, int? timeout = null) where T : class;

Task<IEnumerable<T>> GetWhereAsync<T>(this IDbConnection conn, string where, object? param = null, 
                                      IDbTransaction? tr = null, int? timeout = null) where T : class;
```

#### Insert and Update Methods

Insert new entities or update existing ones.

**Synchronous:**

```csharp
int InsertNoKeyAtt<T>(this IDbConnection connection, T entityToInsert, 
                      IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

int InsertOrUpdate<T>(this IDbConnection conn, T item, 
                      IDbTransaction? tr = null, int? timeout = null) where T : class;

int InsertOrUpdateMerge<T>(this IDbConnection conn, T item, 
                           IDbTransaction? tr = null, int? timeout = null) where T : class;

bool UpdateIdValue<TA, T>(this IDbConnection conn, dynamic id, T value, string field, 
                          IDbTransaction? tr = null, int? timeout = null);

bool UpdateIdValue<TA, T>(this IDbConnection conn, (dynamic id, T value) value, string field, 
                          IDbTransaction? tr = null, int? timeout = null);

bool InsertOrUpdatePartial<T>(this IDbConnection conn, dynamic id, IReadOnlyList<T> items, 
                              IDbTransaction tr, int? timeout = null) where T : class;
```

**Asynchronous:**

```csharp
Task<int> InsertNoKeyAttAsync<T>(this IDbConnection connection, T entityToInsert, 
                                 IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<int> InsertOrUpdateAsync<T>(this IDbConnection conn, T item, 
                                 IDbTransaction? tr = null, int? timeout = null) where T : class;

Task<int> InsertOrUpdateMergeAsync<T>(this IDbConnection conn, T item, 
                                      IDbTransaction? tr = null, int? timeout = null) where T : class;

Task<bool> UpdateIdValueAsync<TA, T>(this IDbConnection conn, dynamic id, T value, string field, 
                                     IDbTransaction? tr = null, int? timeout = null);

Task<bool> UpdateIdValueAsync<TA, T>(this IDbConnection conn, (dynamic id, T value) value, string field, 
                                     IDbTransaction? tr = null, int? timeout = null);

Task<bool> InsertOrUpdatePartialAsync<T>(this IDbConnection conn, dynamic id, IReadOnlyList<T> items, 
                                         IDbTransaction tr, int? timeout = null) where T : class;
```

#### Complex Queries and Partial Key Operations

Advanced operations with JOINs and partial keys.

**Synchronous:**

```csharp
List<T> QueryJoin<T, M>(this IDbConnection conn, string query, object? param, 
                        Action<T, M> join, IDbTransaction? tr = null);

T? GetPartial<T>(this IDbConnection connection, dynamic id1, dynamic id2, 
                 IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

IEnumerable<T> GetPartial<T>(this IDbConnection connection, dynamic id, 
                              IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

IEnumerable<T> GetPartial2<T>(this IDbConnection connection, dynamic id, 
                               IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

IEnumerable<T> GetAllNoKey<T>(this IDbConnection connection, 
                               IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;
```

**Asynchronous:**

```csharp
Task<T?> GetPartialAsync<T>(this IDbConnection connection, dynamic id1, dynamic id2, 
                            IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<IEnumerable<T>> GetPartialAsync<T>(this IDbConnection connection, dynamic id, 
                                        IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<IEnumerable<T>> GetPartial2Async<T>(this IDbConnection connection, dynamic id, 
                                         IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;

Task<IEnumerable<T>> GetAllNoKeyAsync<T>(this IDbConnection connection, 
                                         IDbTransaction? transaction = null, int? commandTimeout = null) where T : class;
```

---

### Custom Attributes

#### ForeignDatabaseAttribute

Mark entities that belong to a different database:

```csharp
[ForeignDatabase("MyOtherDatabase")]
public class Customer
{
    [ExplicitKey]
    public int Id { get; set; }
    public string Name { get; set; }
}
```

**Properties:**
- `string Name` - Name of the foreign database

#### PartialKeyAttribute

Mark properties that form a composite key:

```csharp
public class OrderDetail
{
    [PartialKey(0)]
    public int OrderId { get; set; }
    
    [PartialKey(1)]
    public int ProductId { get; set; }
    
    public decimal Quantity { get; set; }
}
```

**Properties:**
- `int Order` - Order of the partial key (0-based)

---

### Key Description Classes

#### KeyDescription

Immutable class describing an entity's primary key.

**Properties:**

```csharp
string TableName                // Table name
string KeyName                  // Key column name
PropertyInfo KeyProp            // Key property
Type KeyType                    // Key type
bool KeyTypeIsString            // True if key is string
bool IsForeignTable             // True if from foreign database
bool IsAutoKey                  // True if auto-generated
bool IsPartialKey               // True if composite key
string ForeignDatabaseName      // Foreign database name
```

**Methods:**

```csharp
string GetUse()                 // Returns USE statement for foreign DB
object GetKeyValue(object item) // Gets key value from entity
```

#### PartialKeyDescription

Extends `KeyDescription` for composite keys.

**Additional Properties:**

```csharp
string Key2Name                 // Second key column name
PropertyInfo Key2Prop           // Second key property
bool Key2TypeIsString           // True if second key is string
```

**Additional Methods:**

```csharp
object GetKey2Value(object item)            // Gets second key value
string GetAllKeys(object item)              // Returns "key1|key2"
(object, object) DeconstructKey(string key) // Splits composite key
```

---

## Usage Examples

### Basic CRUD Operations

```csharp
using Rop.Dapper.ContribEx10;
using System.Data.SqlClient;

var connection = new SqlConnection(connectionString);

// Delete by key
bool deleted = connection.DeleteByKey<User>(userId);

// Get single entity (optimized)
var user = connection.GetSlim<User>(userId);

// Get multiple entities
var users = connection.GetSome<User>(new[] { 1, 2, 3 });

// Insert or update
var result = connection.InsertOrUpdateMerge(user);

// Update specific field
connection.UpdateIdValue<User, string>(userId, "newEmail@example.com", "Email");
```

### Partial Key Operations

```csharp
// Define entity with partial keys
public class OrderDetail
{
    [PartialKey(0)]
    public int OrderId { get; set; }
    
    [PartialKey(1)]
    public int ProductId { get; set; }
    
    public decimal Quantity { get; set; }
}

// Get by first partial key
var details = connection.GetPartial<OrderDetail>(orderId);

// Get by second partial key
var orders = connection.GetPartial2<OrderDetail>(productId);

// Get specific item by both keys
var detail = connection.GetPartial<OrderDetail>(orderId, productId);

// Replace all details for an order
var newDetails = new List<OrderDetail> { /* ... */ };
connection.InsertOrUpdatePartial(orderId, newDetails, transaction);
```

### Foreign Database Entities

```csharp
[ForeignDatabase("ArchiveDB")]
[Table("Users")]
public class ArchivedUser
{
    [ExplicitKey]
    public int Id { get; set; }
    public string Name { get; set; }
}

// Automatically uses correct database
var archivedUser = connection.GetSlim<ArchivedUser>(userId);
```

### JOIN Queries

```csharp
public class User
{
    [ExplicitKey]
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Order> Orders { get; set; } = new();
}

// Execute JOIN and map results
var users = connection.QueryJoin<User, Order>(
    @"SELECT u.*, o.* 
      FROM Users u 
      INNER JOIN Orders o ON u.Id = o.UserId",
    null,
    (user, order) => user.Orders.Add(order)
);
```

### Async Operations

```csharp
// All methods have async versions
var user = await connection.GetSlimAsync<User>(userId);
var users = await connection.GetAllSlimAsync<User>();
await connection.DeleteByKeyAsync<User>(userId);
var newId = await connection.InsertOrUpdateMergeAsync(user);
```

### Using DapperHelperExtend

```csharp
// Get key information
var keyDesc = DapperHelperExtend.GetKeyDescription<User>();
Console.WriteLine($"Table: {keyDesc.TableName}, Key: {keyDesc.KeyName}");

// Get cached SQL
var sql = DapperHelperExtend.SelectGetSlimCache(typeof(User));

// Format key list
var ids = new[] { 1, 2, 3 };
var idList = DapperHelperExtend.GetIdList(ids); // "1,2,3"
```

---

## Performance Considerations

- **Cached SQL**: All SQL queries are generated once and cached for optimal performance
- **Slim Queries**: Use `GetSlim*` methods to retrieve only necessary columns
- **Batch Operations**: Use `GetSome` instead of multiple `Get` calls
- **Async Operations**: Use async methods for I/O-bound operations
- **Transactions**: Always use transactions for multiple related operations

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

---

## Links

- **GitHub**: [https://github.com/ramoneeza/Rop.Dapper.ContribEx10](https://github.com/ramoneeza/Rop.Dapper.ContribEx10)
- **NuGet**: [https://www.nuget.org/packages/Rop.Dapper.ContribEx10](https://www.nuget.org/packages/Rop.Dapper.ContribEx10)
- **Dapper**: [https://github.com/DapperLib/Dapper](https://github.com/DapperLib/Dapper)

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

*Copyright © 2024 Ramon Ordiales Plaza - Licensed under GPL-3.0-or-later*
