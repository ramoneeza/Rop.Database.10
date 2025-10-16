using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides extension methods for insert, update, and merge operations using Dapper and Dapper.Contrib.
/// </summary>
public static partial class ConnectionHelper
{
    /// <summary>
    /// Inserts an entity of type <typeparamref name="T"/> into the database, ignoring key attributes.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="entityToInsert">Entity to insert.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Number of affected rows.</returns>
    public static int InsertNoKeyAtt<T>(this IDbConnection connection, T entityToInsert,  IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {   var sql = DapperHelperExtend.InsertNoKeyAttCache(typeof(T));
        var r = connection.Execute(sql, entityToInsert, transaction, commandTimeout);
        return r;
    }
    /// <summary>
    /// Inserts an entity of type <typeparamref name="T"/> into the database, ignoring key attributes.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="entityToInsert">Entity to insert.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Task returning the number of affected rows.</returns>
    public static Task<int> InsertNoKeyAttAsync<T>(this IDbConnection connection, T entityToInsert, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var sql = DapperHelperExtend.InsertNoKeyAttCache(typeof(T));
        var r = connection.ExecuteAsync(sql, entityToInsert, transaction, commandTimeout);
        return r;
    }
    /// <summary>
    /// Inserts or updates an entity of type <typeparamref name="T"/> depending on its key value.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="item">Entity to insert or update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>Key value of the entity.</returns>
    public static int InsertOrUpdate<T>(this IDbConnection conn, T item, IDbTransaction? tr = null,int? timeout=null) where T : class
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(T));
        var objkey = kd.GetKeyValue(item);
        if (kd.IsAutoKey)
        {
            var key = (int)objkey;
            if (key <= 0)
            {
                key = (int) conn.Insert(item, tr,timeout);
            }
            else
            {
                conn.Update(item, tr,timeout);
            }
            return key;
        }
        else
        {
            if (objkey is int i)
            {
                var res = conn.Update(item, tr, timeout);
                if (!res) conn.Insert(item, tr, timeout);
                return i;
            }
            else
            {
                i = 1;
                var res = conn.Update(item, tr, timeout);
                if (!res) conn.Insert(item, tr, timeout);
                return i; // i=1 == successful 
            }
        }
    }
    /// <summary>
    /// Inserts or updates an entity of type <typeparamref name="T"/> using a SQL MERGE statement.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="item">Entity to insert or update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>Key value or affected rows.</returns>
    public static int InsertOrUpdateMerge<T>(this IDbConnection conn, T item, IDbTransaction? tr = null, int? timeout = null) where T : class
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(T));
        var objkey = kd.GetKeyValue(item);
        if (kd.IsAutoKey)
        {
            var key = (int)objkey;
            if (key <= 0)
            {
                key = (int) conn.Insert(item, tr,timeout);
            }
            else
            {
                conn.Update(item, tr,timeout);
            }
            return key;
        }
        var sql = DapperHelperExtend.InsertOrUpdateMergeCache(typeof(T));
        var result = conn.Execute(sql, item, tr, timeout);
        if (objkey is int i)
            return (result > 0) ? i : 0;
        else
            return result;
    }
    /// <summary>
    /// Asynchronously inserts or updates an entity of type <typeparamref name="T"/> using a SQL MERGE statement.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="item">Entity to insert or update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>Task returning the key value or affected rows.</returns>
    public static async Task<int> InsertOrUpdateMergeAsync<T>(this IDbConnection conn, T item, IDbTransaction? tr = null, int? timeout = null) where T : class
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(T));
        var objkey = kd.GetKeyValue(item);
        if (kd.IsAutoKey)
        {
            var key = (int)objkey;
            if (key <= 0)
            {
                key = (int)await conn.InsertAsync(item, tr, timeout);
            }
            else
            {
                await conn.UpdateAsync(item, tr, timeout);
            }
            return key;
        }
        var sql = DapperHelperExtend.InsertOrUpdateMergeCache(typeof(T));
        var result = await conn.ExecuteAsync(sql, item, tr, timeout);
        if (objkey is int i)
            return (result > 0) ? i : 0;
        else
            return result;
    }


    /// <summary>
    /// Updates the value of a field for a given id in a table of type <typeparamref name="TA"/>.
    /// </summary>
    /// <typeparam name="TA">Type of the table entity.</typeparam>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="value">Tuple with id and value.</param>
    /// <param name="field">Field name to update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>True if one row was updated, false otherwise.</returns>
    public static bool UpdateIdValue<TA, T>(this IDbConnection conn, (dynamic id, T value) value, string field, IDbTransaction? tr = null, int? timeout = null)
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(TA));
        var sql = $"UPDATE {kd.TableName} SET {field}=@value WHERE {kd.KeyName}=@id";
        var r=conn.Execute(sql, new { id = value.id, value = value.value }, tr,timeout);
        return r == 1;
    }
    /// <summary>
    /// Updates the value of a field for a given id in a table of type <typeparamref name="TA"/>.
    /// </summary>
    /// <typeparam name="TA">Type of the table entity.</typeparam>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Id value.</param>
    /// <param name="value">Value to update.</param>
    /// <param name="field">Field name to update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>True if one row was updated, false otherwise.</returns>
    public static bool UpdateIdValue<TA, T>(this IDbConnection conn, dynamic id, T value, string field, IDbTransaction? tr = null, int? timeout = null)
    {
        return UpdateIdValue<TA, T>(conn, (id, value), field, tr, timeout);
    }
    
    // Async

    /// <summary>
    /// Asynchronously inserts or updates an entity of type <typeparamref name="T"/> depending on its key value.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="item">Entity to insert or update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>Key value of the entity.</returns>
    public static async Task<int> InsertOrUpdateAsync<T>(this IDbConnection conn, T item, IDbTransaction? tr = null,int? timeout=null) where T : class
    {
        return await Task.Run(() => conn.InsertOrUpdate(item, tr, timeout));

    }
    /// <summary>
    /// Asynchronously updates the value of a field for a given id in a table of type <typeparamref name="TA"/>.
    /// </summary>
    /// <typeparam name="TA">Type of the table entity.</typeparam>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="value">Tuple with id and value.</param>
    /// <param name="field">Field name to update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>True if one row was updated, false otherwise.</returns>
    public static async Task<bool> UpdateIdValueAsync<TA, T>(this IDbConnection conn, (dynamic id, T value) value, string field, IDbTransaction? tr = null,int? timeout=null)
    {
        return await Task.Run(() => UpdateIdValue<TA, T>(conn, value, field, tr, timeout));
    }
    /// <summary>
    /// Asynchronously updates the value of a field for a given id in a table of type <typeparamref name="TA"/>.
    /// </summary>
    /// <typeparam name="TA">Type of the table entity.</typeparam>
    /// <typeparam name="T">Type of the value.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Id value.</param>
    /// <param name="value">Value to update.</param>
    /// <param name="field">Field name to update.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>True if one row was updated, false otherwise.</returns>
    public static async Task<bool> UpdateIdValueAsync<TA, T>(this IDbConnection conn, dynamic id, T value, string field, IDbTransaction? tr = null, int? timeout = null)
    {
        return await Task.Run(() => UpdateIdValue<TA, T>(conn,id, value, field, tr, timeout));
    }

    /// <summary>
    /// Inserts or updates a list of entities of type <typeparamref name="T"/> using a partial key, deleting previous entries with the same key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Partial key value.</param>
    /// <param name="items">List of entities to insert or update.</param>
    /// <param name="tr">Transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>True if all items were inserted, false otherwise.</returns>
    public static bool InsertOrUpdatePartial<T>(this IDbConnection conn, dynamic id, IReadOnlyList<T> items, IDbTransaction tr, int? timeout = null) where T: class
    {
        
        var kd = DapperHelperExtend.GetPartialKeyDescription(typeof(T));
        var tablename=kd.TableName;
        var key1 = kd.KeyName;
        conn.Execute($"DELETE FROM {tablename} WHERE {key1}=@id", new { id = id }, tr, timeout);
        var cnt = 0;
        foreach (var item in items)
        {
            kd.KeyProp.SetValue(item,id);
            cnt+=InsertNoKeyAtt<T>(conn, item, tr, timeout);
        }
        return cnt == items.Count;
    }
    /// <summary>
    /// Asynchronously inserts or updates a list of entities of type <typeparamref name="T"/> using a partial key, deleting previous entries with the same key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Partial key value.</param>
    /// <param name="items">List of entities to insert or update.</param>
    /// <param name="tr">Transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>Task returning true if all items were inserted, false otherwise.</returns>
    public static async Task<bool> InsertOrUpdatePartialAsync<T>(this IDbConnection conn, dynamic id, IReadOnlyList<T> items, IDbTransaction tr, int? timeout = null) where T : class
    {

        var kd = DapperHelperExtend.GetPartialKeyDescription(typeof(T));
        var tablename = kd.TableName;
        var key1 = kd.KeyName;
        await conn.ExecuteAsync($"DELETE FROM {tablename} WHERE {key1}=@id", new { id = id }, tr, timeout);
        var cnt = 0;
        foreach (var item in items)
        {
            kd.KeyProp.SetValue(item, id);
            cnt += await InsertNoKeyAttAsync<T>(conn, item, tr, timeout);
        }
        return cnt == items.Count;
    }
}