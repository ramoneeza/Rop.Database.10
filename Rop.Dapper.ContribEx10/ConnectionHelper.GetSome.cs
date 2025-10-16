using System.Collections;
using System.Data;
using Dapper;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides extension methods for querying entities by key lists and custom WHERE clauses using Dapper.
/// </summary>
public static partial class ConnectionHelper
{
    /// <summary>
    /// Gets a list of entities of type <typeparamref name="T"/> whose keys are in the provided list.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="ids">List of key values.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetSome<T>(this IDbConnection conn, IEnumerable ids, IDbTransaction? tr = null, int? commandTimeout = null) where T : class
    {
        var lst = DapperHelperExtend.GetIdListDyn(ids);
        if (string.IsNullOrWhiteSpace(lst)) return new List<T>();
        var keyd = DapperHelperExtend.GetAnyKeyDescription(typeof(T))??throw new ArgumentException($"Type {typeof(T)} has not valid keys");
        return conn.Query<T>($"SELECT * FROM {keyd.TableName} WHERE {keyd.KeyName} IN ({lst})", null, tr,true,commandTimeout);
    }

    /// <summary>
    /// Gets a list of entities of type <typeparamref name="T"/> matching the specified WHERE clause.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="where">WHERE clause string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetWhere<T>(this IDbConnection conn, string where, object? param=null, IDbTransaction? tr = null, int? commandTimeout = null) where T : class
    {
        var keyd = DapperHelperExtend.GetAnyKeyDescription(typeof(T)) ?? throw new ArgumentException($"Type {typeof(T)} has not valid keys");
        return conn.Query<T>($"SELECT * FROM {keyd.TableName} WHERE {@where}",param, tr,true,commandTimeout);
    }
    
    // Async

    /// <summary>
    /// Asynchronously gets a list of entities of type <typeparamref name="T"/> whose keys are in the provided list.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="ids">List of key values.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static async Task<IEnumerable<T>> GetSomeAsync<T>(this IDbConnection conn, IEnumerable ids, IDbTransaction? tr = null,int? timeout=null) where T : class
    {
        var lst = DapperHelperExtend.GetIdListDyn(ids);
        if (string.IsNullOrWhiteSpace(lst)) return new List<T>();
        var keyd = DapperHelperExtend.GetAnyKeyDescription(typeof(T)) ?? throw new ArgumentException($"Type {typeof(T)} has not valid keys");
        var q= await conn.QueryAsync<T>($"SELECT * FROM {keyd.TableName} WHERE {keyd.KeyName} IN ({lst})", null, tr, timeout);
        return q;
    }
    
    /// <summary>
    /// Asynchronously gets a list of entities of type <typeparamref name="T"/> matching the specified WHERE clause.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="where">WHERE clause string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="timeout">Optional command timeout.</param>
    /// <returns>List of entities of type <typeparamref name="T"/>.</returns>
    public static async Task<IEnumerable<T>> GetWhereAsync<T>(this IDbConnection conn, string where, object? param=null, IDbTransaction? tr = null,int? timeout=null) where T : class
    {
        var keyd = DapperHelperExtend.GetAnyKeyDescription(typeof(T)) ?? throw new ArgumentException($"Type {typeof(T)} has not valid keys");
        var q = await conn.QueryAsync<T>($"SELECT * FROM {keyd.TableName} WHERE {@where}", param, tr, timeout);
        return q;
    }
}