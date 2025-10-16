/*
 * -----------------------------------------------------------------------------
 * <ramon@eeza.csic.es> wrote this file. As long as you retain this notice 
 * you can do whatever you want with this stuff. If we meet some day, and you 
 * think this stuff is worth it, you can buy me a coffee in return.
 * -----------------------------------------------------------------------------
 *
 * File: AbsDatabase.cs 
 *
 * Abstract base class for database access.
 * 
 * Copyright (c) 2024 Ramon Ordiales Plaza
 */


using System.Collections;
using System.Data;
using System.Data.Common;

namespace Rop.Database;


/// <summary>
/// AbsDatabase class. Abstract base class for database access.
/// </summary>
public abstract partial class AbsDatabase 
{
    #region Protected Fields
    /// <summary>
    /// The connection string.
    /// </summary>
    protected readonly string Strconn;

    #endregion
    /// <summary>
    /// Server Name
    /// </summary>
    public string Server { get; } = "";
    /// <summary>
    /// Main Database Name
    /// </summary>
    public string MainDatabaseName { get; } = "";

    /// <summary>
    /// Constructor with connection string.
    /// </summary>
    /// <param name="strconn">Connection string to the database.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strconn"/> is null.</exception>
    protected AbsDatabase(string strconn)
    {
        Strconn = strconn ?? throw new ArgumentNullException(nameof(strconn));
        // ReSharper disable once VirtualMemberCallInConstructor
        var (dataSource, initialCatalog) = ExtractConn(strconn);
        Server = dataSource ?? "";
        MainDatabaseName = initialCatalog ?? "";
    }
    /// <summary>
    /// Extracts the DataSource and InitialCatalog from the connection string.
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    protected virtual (string? datasource,string? initialcatalog) ExtractConn(string? conn)
    {
        return ConnStringExtractor.Extract(conn);
    }
    /// <summary>
    /// Abstract Factory Method to create a database connection.
    /// </summary>
    /// <returns>An open or ready-to-open <see cref="DbConnection"/> instance.</returns>
    public abstract DbConnection FactoryConnection();

    /// <summary>
    /// Insert Item
    /// </summary>
    /// <typeparam name="T">Entity type to insert.</typeparam>
    /// <param name="item">Entity instance to insert.</param>
    /// <returns>Result with the inserted key or affected rows as integer.</returns>
    public Result<int> Insert<T>(T item) where T:class
    {
        var res = UnitOfWorkSingle(conn => (int) conn.Insert(item));
        return res;
    }
    /// <summary>
    /// Update Item
    /// </summary>
    /// <typeparam name="T">Entity type to update.</typeparam>
    /// <param name="item">Entity instance with updated values.</param>
    /// <returns>Result indicating whether the update succeeded.</returns>
    public Result<bool> Update<T>(T item) where T:class
    {
        return UnitOfWorkSingle<bool>(conn => conn.Update(item));
    }
    /// <summary>
    /// InsertOrUpdate Item
    /// </summary>
    /// <typeparam name="T">Entity type to insert or update.</typeparam>
    /// <param name="item">Entity instance to insert or update.</param>
    /// <returns>Result with the key or affected rows as integer.</returns>
    public Result<int> InsertOrUpdate<T>(T item) where T:class
    {
        return UnitOfWorkSingle(conn => conn.InsertOrUpdate(item));
    }
    /// <summary>
    /// Query Items
    /// </summary>
    /// <typeparam name="T">Element type to return.</typeparam>
    /// <param name="query">SQL query text.</param>
    /// <param name="param">Optional parameters for the query.</param>
    /// <returns>Enumerable result with the list of items.</returns>
    public EnumerableResult<T> Query<T>(string query,object? param=null)
    {
        return UnitOfWork(conn => conn.Query<T>(query,param).ToList());
    }
    /// <summary>
    /// Join Items
    /// </summary>
    /// <typeparam name="T">Primary entity type.</typeparam>
    /// <typeparam name="M">Joined entity type.</typeparam>
    /// <param name="query">SQL join query.</param>
    /// <param name="param">Optional parameters for the query.</param>
    /// <param name="join">Action that maps/join the two types.</param>
    /// <returns>Enumerable result with the joined primary entities.</returns>
    public EnumerableResult<T> QueryJoin<T,M>(string query,object? param,Action<T,M> join)
    {
        return UnitOfWork(conn => conn.QueryJoin(query, param, join));
    }
    /// <summary>
    /// Query One Item
    /// </summary>
    /// <typeparam name="T">Entity type to return.</typeparam>
    /// <param name="query">SQL query expected to return a single row.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <returns>Result with a single item or null if not found.</returns>
    public Result<T> QueryOne<T>(string query,object? param=null)
    {
        return new(UnitOfWorkSingle(conn => conn.QueryFirstOrDefault<T>(query, param)).Value);
    }
    /// <summary>
    /// Execute Query
    /// </summary>
    /// <param name="query">SQL command to execute (INSERT/UPDATE/DELETE or other).</param>
    /// <param name="param">Optional parameters for the command.</param>
    /// <returns>Result with the number of affected rows.</returns>
    public Result<int> Execute(string query, object? param = null)
    {
        return UnitOfWorkSingle(conn => conn.Execute(query, param));
    }
    /// <summary>
    /// Execute Scalar Query
    /// </summary>
    /// <typeparam name="T">Type of the scalar result.</typeparam>
    /// <param name="query">SQL scalar query.</param>
    /// <param name="param">Optional parameters for the query.</param>
    /// <returns>Result with the scalar value.</returns>
    public Result<T> ExecuteScalar<T>(string query,object? param = null)
    {
        return new(UnitOfWorkSingle(conn => conn.ExecuteScalar<T>(query, param)).Value);
    }
    /// <summary>
    /// Check Table
    /// </summary>
    /// <typeparam name="T">Entity type whose table will be checked.</typeparam>
    /// <returns>VoidResult indicating success or failure of the check.</returns>
    public VoidResult Check<T>() where T:class
    {
        try
        {
            var td = DapperHelperExtend.GetKeyDescription<T>();
            if (td is null) return Error.Fail("Table Key Description not found");
            string t;
            if (td.IsForeignTable && !this.MainDatabaseName.Equals(td.ForeignDatabaseName,StringComparison.OrdinalIgnoreCase))
                t = $"{td.ForeignDatabaseName}.dbo.{td.TableName}";
            else
                t=td.TableName;
            return UnitOfWorkSingle(conn => conn.QueryFirstOrDefault<T>($"SELECT TOP(1) * FROM {t}"));
        }
        catch (Exception ex)
        {
            return Error.Exception(ex);
        }
    }
    /// <summary>
    /// Get Item by Id
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="id">Integer key value.</param>
    /// <returns>Result with the entity or null if not found.</returns>
    public Result<T> Get<T>(int id) where T:class
    {
        return Get<int, T>(id);
    }
    /// <summary>
    /// Get Item by Id
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="id">String key value.</param>
    /// <returns>Result with the entity or null if not found.</returns>
    public Result<T> Get<T>(string id) where T : class
    {
        return Get<string, T>(id);
    }
    /// <summary>
    /// Get Item by Id
    /// </summary>
    /// <typeparam name="K">Key type.</typeparam>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="id">Key value.</param>
    /// <returns>Result with the entity or null if not found.</returns>
    public Result<T> Get<K,T>(K id) where T : class where K : notnull
    {
        return UnitOfWorkSingle(conn => conn.Get<T>(id));
    }
    
    
    /// <summary>
    /// GetAllItems
    /// </summary>
    /// <typeparam name="T">Entity type to enumerate.</typeparam>
    /// <returns>Enumerable result with all entities of the specified type.</returns>
    public EnumerableResult<T> GetAll<T>() where T:class
    {
        return UnitOfWork(conn => conn.GetAll<T>().ToList());
    }
    /// <summary>
    /// GetAllItemsAsync
    /// </summary>
    /// <typeparam name="T">Entity type to enumerate.</typeparam>
    /// <returns>Task with an enumerable result of all entities.</returns>
    public Task<EnumerableResult<T>> GetAllAsync<T>() where T : class
    {
        return UnitOfWorkAsync(conn => conn.GetAllAsync<T>());
    }
    /// <summary>
    /// Insert Item Async
    /// </summary>
    /// <typeparam name="T">Entity type to insert.</typeparam>
    /// <param name="item">Entity instance to insert.</param>
    /// <returns>Task result with the inserted key or affected rows as integer.</returns>
        
    public Task<Result<int>> InsertAsync<T>(T item) where T : class
    {
        return UnitOfWorkSingleAsync<int>((conn,tr) =>conn.InsertAsync(item,tr));
    }
    /// <summary>
    /// Update Item Async
    /// </summary>
    /// <typeparam name="T">Entity type to update.</typeparam>
    /// <param name="item">Entity instance with updated values.</param>
    /// <returns>Task result indicating whether the update succeeded.</returns>
    public Task<Result<bool>> UpdateAsync<T>(T item) where T : class
    {
        return UnitOfWorkSingleAsync<bool>((conn,tr) =>conn.UpdateAsync(item,tr));
    }
    /// <summary>
    /// InsertOrUpdate Item Async
    /// </summary>
    /// <typeparam name="T">Entity type to insert or update.</typeparam>
    /// <param name="item">Entity instance to insert or update.</param>
    /// <returns>Task result with the key or affected rows as integer.</returns>
    public Task<Result<int>> InsertOrUpdateAsync<T>(T item) where T : class
    {
        return UnitOfWorkSingleAsync((conn,tr) => conn.InsertOrUpdateAsync(item, tr));
    }
    /// <summary>
    /// Query Items Async
    /// </summary>
    /// <typeparam name="T">Element type returned by the query.</typeparam>
    /// <param name="query">SQL query text.</param>
    /// <param name="param">Optional parameters for the query.</param>
    /// <returns>Task with an enumerable result of items.</returns>
    public Task<EnumerableResult<T>> QueryAsync<T>(string query, object? param = null)
    {
        return UnitOfWorkAsync(conn => conn.QueryAsync<T>(query, param));
    }
    /// <summary>
    /// Query One Item Async
    /// </summary>
    /// <typeparam name="T">Entity type to return.</typeparam>
    /// <param name="query">SQL query expected to return a single row.</param>
    /// <param name="param">Optional query parameters.</param>
    /// <returns>Task result with a single item or null if not found.</returns>
    public Task<Result<T>> QueryOneAsync<T>(string query,object? param = null)
    {
        return UnitOfWorkSingleAsync(conn => conn.QueryFirstOrDefaultAsync<T>(query, param));
    }
    /// <summary>
    /// Execute Query Async
    /// </summary>
    /// <param name="query">SQL command to execute.</param>
    /// <param name="param">Optional parameters for the command.</param>
    /// <returns>Task result with the number of affected rows.</returns>
        
    public Task<Result<int>> ExecuteAsync(string query, object? param = null)
    {
        return UnitOfWorkSingleAsync(conn => conn.ExecuteAsync(query, param));
    }
        /// <summary>
        /// Execute Scalar Query Async
        /// </summary>
        /// <typeparam name="T">Type of the scalar result.</typeparam>
        /// <param name="query">SQL scalar query.</param>
        /// <param name="param">Optional parameters for the query.</param>
        /// <returns>Task result with the scalar value.</returns>
    public Task<Result<T>> ExecuteScalarAsync<T>(string query, object? param = null)
    {
        return UnitOfWorkSingleAsync(conn => conn.ExecuteScalarAsync<T>(query, param));
    }
        /// <summary>
        /// Get Item by Id Async
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="id">Integer key value.</param>
        /// <returns>Task result with the entity or null if not found.</returns>
    public Task<Result<T>> GetAsync<T>(int id) where T : class
    {
        return GetAsync<int, T>(id);
    }
        /// <summary>
        /// Get Item by Id Async
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="id">String key value.</param>
        /// <returns>Task result with the entity or null if not found.</returns>
    public Task<Result<T>> GetAsync<T>(string id) where T : class
    {
        return GetAsync<string, T>(id);
    }
        /// <summary>
        /// Get Item by Id Async
        /// </summary>
        /// <typeparam name="K">Key type.</typeparam>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="id">Key value.</param>
        /// <returns>Task result with the entity or null if not found.</returns>
    public Task<Result<T>> GetAsync<K,T>(K id) where T : class where K:notnull
    {
        return UnitOfWorkSingleAsync(async conn =>await conn.GetAsync<T>(id));
    }
        

    /// <summary>
    /// Delete Item by Item
    /// </summary>
    /// <typeparam name="T">Entity type to delete.</typeparam>
    /// <param name="item">Entity instance to delete.</param>
    /// <returns>Result indicating whether the deletion succeeded.</returns>
    public Result<bool> Delete<T>(T item) where T : class
    {
        return UnitOfWorkSingle<bool>(c=>c.Delete(item));
    }

    /// <summary>
        /// Delete Item Async
        /// </summary>
        /// <typeparam name="T">Entity type to delete.</typeparam>
        /// <param name="item">Entity instance to delete.</param>
        /// <returns>Task result indicating whether the deletion succeeded.</returns>
        
    public Task<Result<bool>> DeleteAsync<T>(T item) where T : class
    {
        return UnitOfWorkSingleAsync<bool>(conn => conn.DeleteAsync(item));
    }
    /// <summary>
    /// Delete Item by Key
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">Integer key value.</param>
    /// <returns>Result indicating whether the deletion succeeded.</returns>
    public Result<bool> Delete<T>(int key) where T : class
    {
        return Delete<int, T>(key);
    }
    /// <summary>
    /// Delete Item by Key Async
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">Integer key value.</param>
    /// <returns>Task result indicating whether the deletion succeeded.</returns>
    public Task<Result<bool>> DeleteAsync<T>(int key) where T : class
    {
        return DeleteAsync<int, T>(key);
    }
    /// <summary>
    /// Delete Item by Key
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">String key value.</param>
    /// <returns>Result indicating whether the deletion succeeded.</returns>
    public Result<bool> Delete<T>(string key) where T : class
    {
        return Delete<string, T>(key);
    }
    /// <summary>
    /// Delete Item by Key Async
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">String key value.</param>
    /// <returns>Task result indicating whether the deletion succeeded.</returns>
        
    public Task<Result<bool>> DeleteAsync<T>(string key) where T : class
    {
        return DeleteAsync<string, T>(key);
    }
    /// <summary>
    /// Delete Item by Key
    /// </summary>
    /// <typeparam name="K">Key type.</typeparam>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">Key value.</param>
    /// <returns>Result indicating whether the deletion succeeded.</returns>
    public Result<bool> Delete<K,T>(K key) where T : class where K: notnull
    {
        return UnitOfWorkSingle<bool>(conn =>conn.DeleteByKey<T>(key));
    }
    /// <summary>
    /// Delete Item by Key Async
    /// </summary>
    /// <typeparam name="K">Key type.</typeparam>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="key">Key value.</param>
    /// <returns>Task result indicating whether the deletion succeeded.</returns>
    public async Task<Result<bool>> DeleteAsync<K,T>(K key) where T : class where K : notnull
    {
        return await UnitOfWorkSingleAsync<bool>(conn =>conn.DeleteByKeyAsync<T>(key));
    }

    
    /// <summary>
    /// Update Single Value by Id
    /// </summary>
    /// <typeparam name="DB">Type used to resolve table metadata.</typeparam>
    /// <typeparam name="T">Type of the value to set.</typeparam>
    /// <param name="id">Key value identifying the row.</param>
    /// <param name="value">New value to assign.</param>
    /// <param name="field">Field name to update.</param>
    /// <returns>Result indicating whether exactly one row was updated.</returns>
    public Result<bool> UpdateIdValue<DB, T>(object id, T value, string field)
    {
        return UnitOfWorkSingle<bool>(conn=>conn.UpdateIdValue<DB, T>(id,value, field));
    }
    /// <summary>
    /// Update Single Value by Id
    /// </summary>
    /// <typeparam name="DB">Type used to resolve table metadata.</typeparam>
    /// <typeparam name="T">Type of the value to set.</typeparam>
    /// <param name="value">Tuple with id and value.</param>
    /// <param name="field">Field name to update.</param>
    /// <returns>Result indicating whether exactly one row was updated.</returns>
        
    public Result<bool> UpdateIdValue<DB, T>((object id, T value) value, string field)
    {
        return UnitOfWorkSingle<bool>(conn=>conn.UpdateIdValue<DB, T>(value.id,value.value, field));
    }
    
    

    /// <summary>
    /// Get Some Items by list of Ids
    /// </summary>
    /// <typeparam name="K">Key type.</typeparam>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">Enumerable of key values to fetch.</param>
    /// <returns>Enumerable result with the matching entities.</returns>
    public EnumerableResult<T> GetSome<K,T>( IEnumerable<K> ids) where T : class where K : notnull
    {
        return UnitOfWork(conn => conn.GetSome<T>(ids));
    }
    /// <summary>
    /// Get Some Items by list of Ids Async
    /// </summary>
    /// <typeparam name="K">Key type.</typeparam>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">Enumerable of key values to fetch.</param>
    /// <returns>Task with an enumerable result of the matching entities.</returns>
    public async Task<EnumerableResult<T>> GetSomeAsync<K,T>(IEnumerable<K> ids) where T : class where K : notnull
    {
        var r= await UnitOfWorkAsync<T>(async conn =>await conn.GetSomeAsync<T>(ids));
        return r;
    }

    /// <summary>
    /// Get Some Items by list of Ids
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">List of integer keys.</param>
    /// <returns>Enumerable result with the matching entities.</returns>
    public EnumerableResult<T> GetSome<T>( IEnumerable<int> ids) where T : class
    {
        return GetSome<int, T>(ids);
    }

    /// <summary>
    /// Get Some Items by list of Ids Async
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">List of integer keys.</param>
    /// <returns>Task with an enumerable result of the matching entities.</returns>
    public Task<EnumerableResult<T>> GetSomeAsync<T>(IEnumerable<int> ids) where T : class
    {
        return GetSomeAsync<int, T>(ids);
    }
    /// <summary>
    /// Get Some Items by list of Ids
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">List of string keys.</param>
    /// <returns>Enumerable result with the matching entities.</returns>
    public EnumerableResult<T> GetSome<T>(IEnumerable<string> ids) where T : class
    {
        return GetSome<string, T>(ids);
    }
    /// <summary>
    /// Get Some Items by list of Ids Async
    /// </summary>
    /// <typeparam name="T">Entity type to retrieve.</typeparam>
    /// <param name="ids">List of string keys.</param>
    /// <returns>Task with an enumerable result of the matching entities.</returns>
        
    public Task<EnumerableResult<T>> GetSomeAsync<T>(IEnumerable<string> ids) where T : class
    {
        return GetSomeAsync<string, T>(ids);
    }
    /// <summary>
    /// GetKeyDescription of Item Type
    /// </summary>
    /// <typeparam name="T">Type to inspect.</typeparam>
    /// <returns>Key description for the specified type, or null if not available.</returns>
        
    public KeyDescription? GetKeyDescription<T>()
    {
        return GetKeyDescription(typeof(T));
    }
    /// <summary>
    /// Retrieves a partial key description for the specified type.
    /// </summary>
    /// <typeparam name="T">Type for which the partial key description is requested.</typeparam>
    /// <returns>A <see cref="PartialKeyDescription"/> or null when none is available.</returns>
    public PartialKeyDescription? GetPartialKeyDescription<T>() => GetPartialKeyDescription(typeof(T));

    /// <summary>
    /// Retrieves the key description of any kind for the specified type <typeparamref name="T"/>.
    /// If a full key description is not available, it attempts to retrieve a partial key description.
    /// </summary>
    /// <typeparam name="T">The type for which to retrieve the key description.</typeparam>
    /// <returns>
    /// A <see cref="KeyDescription"/> instance if a full key description is available; otherwise, a
    /// <see cref="PartialKeyDescription"/> instance if a partial key description is available; or <c>null</c>.
    /// </returns>
    public KeyDescription? GetAnyKeyDescription<T>() => GetAnyKeyDescription(typeof(T));

    /// <summary>
    /// GetKeyDescription of Item Type
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <returns>Key description for the specified type, or null if not available.</returns>
    public KeyDescription? GetKeyDescription(Type type) => DapperHelperExtend.GetKeyDescription(type);
    /// <summary>
    /// Retrieves the key description of any kind for the specified type.
    /// </summary>
    /// <param name="type">The type for which the key description is to be retrieved.</param>
    /// <returns>
    /// A <see cref="KeyDescription"/> instance containing the key description of the specified type, 
    /// or <c>null</c> if no key description is available.
    /// </returns>
    public KeyDescription? GetAnyKeyDescription(Type type) => DapperHelperExtend.GetAnyKeyDescription(type);

    /// <summary>
    /// Retrieves a partial key description for the specified type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which the partial key description is to be retrieved.</param>
    /// <returns>
    /// A <see cref="PartialKeyDescription"/> object representing the partial key description of the specified type,
    /// or <c>null</c> if no partial key description is available.
    /// </returns>
    public PartialKeyDescription? GetPartialKeyDescription(Type type) => DapperHelperExtend.GetPartialKeyDescription(type);

    /// <summary>
    /// GetKeys from Table
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="K">Key type to return.</typeparam>
    /// <returns>Enumerable result with the key values from the table.</returns>
    public EnumerableResult<K> GetKeys<T, K>()  where K : notnull
    {
        try
        {
            var kd = GetKeyDescription<T>();
            if (kd == null) return Error.Fail("Key Description not found");
            var f = kd.KeyName;
            var tb = kd.TableName;
            return UnitOfWork(conn => conn.Query<K>($"SELECT {f} FROM {tb}").ToList());
        }
        catch (Exception ex)
        {
            return Error.Exception(ex);
        }
    }
}