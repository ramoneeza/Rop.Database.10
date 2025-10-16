using System.Collections;
using System.Data;
using Dapper;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides extension methods for optimized SELECT queries using Dapper.
/// </summary>
public static partial class ConnectionHelper
{
    /// <summary>
    /// Gets a single entity of type <typeparamref name="T"/> using a slim SELECT query by key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Entity of type <typeparamref name="T"/> or null if not found.</returns>
    public static T? GetSlim<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetSlimCache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var obj = connection.Query<T>(sql, dynParams, transaction, commandTimeout: commandTimeout).FirstOrDefault();
        return obj;
    }
    
    /// <summary>
    /// Gets all entities of type <typeparamref name="T"/> using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetAllSlim<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetAllSlimCache(type);
        var result = connection.Query<T>(sql,null,transaction,true,commandTimeout);
        return result;
    }
    /// <summary>
    /// Gets a list of entities of type <typeparamref name="T"/> whose keys are in the provided list, using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="ids">List of key values.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static List<T> GetSomeSlim<T>(this IDbConnection conn, IEnumerable ids, IDbTransaction? tr = null) where T : class
    {
        var lst = DapperHelperExtend.GetIdListDyn(ids);
        var keyd = DapperHelperExtend.GetKeyDescription(typeof(T));
        var sql = DapperHelperExtend.SelectGetAllSlimCache(typeof(T));
        return conn.Query<T>($"{sql} WHERE {keyd.KeyName} IN ({lst})", null, tr).ToList();
    }

    /// <summary>
    /// Gets a list of entities of type <typeparamref name="T"/> matching the specified WHERE clause, using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="where">WHERE clause string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static List<T> GetWhereSlim<T>(this IDbConnection conn, string where, object? param=null, IDbTransaction? tr = null) where T : class
    {
        var sql = DapperHelperExtend.SelectGetAllSlimCache(typeof(T));
        return conn.Query<T>($"{sql} WHERE {@where}",param, tr).ToList();
    }

    // Async

    /// <summary>
    /// Asynchronously gets a single entity of type <typeparamref name="T"/> using a slim SELECT query by key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Entity of type <typeparamref name="T"/> or null if not found.</returns>
    public static async Task<T?> GetSlimAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetSlimCache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, dynParams,transaction,commandTimeout);
    }
    /// <summary>
    /// Asynchronously gets all entities of type <typeparamref name="T"/> using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static async Task<IEnumerable<T>> GetAllSlimAsync<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetAllSlimCache(type);
        var result = await connection.QueryAsync<T>(sql,null,transaction,commandTimeout);
        return result;
    }
    /// <summary>
    /// Asynchronously gets a list of entities of type <typeparamref name="T"/> whose keys are in the provided list, using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="ids">List of key values.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static async Task<IEnumerable<T>> GetSomeSlimAsync<T>(this IDbConnection conn, IEnumerable ids, IDbTransaction? tr = null) where T : class
    {
        var lst = DapperHelperExtend.GetIdListDyn(ids);
        var keyd = DapperHelperExtend.GetKeyDescription(typeof(T));
        var sql = DapperHelperExtend.SelectGetAllSlimCache(typeof(T));
        var r = await conn.QueryAsync<T>($"{sql} WHERE {keyd.KeyName} IN ({lst})", null, tr);
        return r;
    }
    /// <summary>
    /// Asynchronously gets a list of entities of type <typeparamref name="T"/> matching the specified WHERE clause, using a slim SELECT query.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="where">WHERE clause string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static async Task<IEnumerable<T>> GetWhereSlimAsync<T>(this IDbConnection conn, string where, object? param=null, IDbTransaction? tr = null) where T : class
    {
        var sql = DapperHelperExtend.SelectGetAllSlimCache(typeof(T));
        var r=await conn.QueryAsync<T>($"{sql} WHERE {@where}",param, tr);
        return r;
    }
}