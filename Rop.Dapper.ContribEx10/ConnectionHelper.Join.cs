using System.Collections;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides extension methods for advanced join and partial key queries using Dapper.
/// </summary>
public static partial class ConnectionHelper
{
    /// <summary>
    /// Executes a join query and maps the results using the provided join action.
    /// </summary>
    /// <typeparam name="T">Type of the main entity.</typeparam>
    /// <typeparam name="M">Type of the joined entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="query">SQL query string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="join">Action to join entities.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of joined entities of type <typeparamref name="T"/>.</returns>
    public static List<T> QueryJoin<T, M>(this IDbConnection conn, string query, object? param, Action<T, M> join, IDbTransaction? tr = null)
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(T));
        if (kd.KeyTypeIsString)
            return _queryJoin<T, M, string>(conn, query, param, join, tr);
        else
            return _queryJoin<T, M, int>(conn, query, param, join, tr);
    }
    /// <summary>
    /// Internal helper for executing join queries with a specific key type.
    /// </summary>
    /// <typeparam name="T">Type of the main entity.</typeparam>
    /// <typeparam name="M">Type of the joined entity.</typeparam>
    /// <typeparam name="K">Type of the key.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="query">SQL query string.</param>
    /// <param name="param">Query parameters.</param>
    /// <param name="join">Action to join entities.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <returns>List of joined entities of type <typeparamref name="T"/>.</returns>
    private static List<T> _queryJoin<T, M, K>(IDbConnection conn, string query, object? param, Action<T, M> join, IDbTransaction? tr = null) where K: notnull
    {
        var kd = DapperHelperExtend.GetKeyDescription(typeof(T));
        if (kd==null || kd.KeyType!=typeof(K)) throw new ArgumentException($"Type {typeof(T)} has not valid key of type {typeof(K)}");
        var kd2 = DapperHelperExtend.GetAnyKeyDescription(typeof(M));
        if (kd2==null) throw new ArgumentException($"Type {typeof(M)} has not valid key");
        var dic = new Dictionary<K, T>();
        var res1 = conn.Query<T, M, T>(query, map: (t, m) =>
        {
            var key = (K)DapperHelperExtend.GetKeyValue(t);
            if (!dic.TryGetValue(key, out var v))
            {
                v = t;
                dic[key] = t;
            }
            @join(v, m);
            return v;
        }, param: param, splitOn: kd2.KeyName, transaction: tr).ToList();
        return dic.Values.ToList();
    }

    /// <summary>
    /// Gets a single entity of type <typeparamref name="T"/> using two partial keys.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id1">First key value.</param>
    /// <param name="id2">Second key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Entity of type <typeparamref name="T"/> or null if not found.</returns>
    public static T? GetPartial<T>(this IDbConnection connection, dynamic id1,dynamic id2, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartial12Cache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id1", id1);
        dynParams.Add("@id2", id2);
        var obj = connection.QueryFirstOrDefault<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }
    /// <summary>
    /// Asynchronously gets a single entity of type <typeparamref name="T"/> using two partial keys.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id1">First key value.</param>
    /// <param name="id2">Second key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Task returning the entity of type <typeparamref name="T"/> or null if not found.</returns>
    public static Task<T?> GetPartialAsync<T>(this IDbConnection connection, dynamic id1, dynamic id2, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartial12Cache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id1", id1);
        dynParams.Add("@id2", id2);
        var obj = connection.QueryFirstOrDefaultAsync<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }

    /// <summary>
    /// Gets entities of type <typeparamref name="T"/> using a partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetPartial<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartialCache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var obj = connection.Query<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }
    /// <summary>
    /// Asynchronously gets entities of type <typeparamref name="T"/> using a partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Task returning an enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static Task<IEnumerable<T>> GetPartialAsync<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartialCache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var obj = connection.QueryAsync<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }

    /// <summary>
    /// Gets entities of type <typeparamref name="T"/> using the second partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Second key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetPartial2<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartial2Cache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var obj = connection.Query<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }
    /// <summary>
    /// Asynchronously gets entities of type <typeparamref name="T"/> using the second partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="id">Second key value.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Task returning an enumerable of entities of type <typeparamref name="T"/>.</returns>
    public static Task<IEnumerable<T>> GetPartial2Async<T>(this IDbConnection connection, dynamic id, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var type = typeof(T);
        var sql = DapperHelperExtend.SelectGetPartial2Cache(type);
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var obj = connection.QueryAsync<T>(sql, dynParams, transaction, commandTimeout: commandTimeout);
        return obj;
    }

    /// <summary>
    /// Gets all entities of type <typeparamref name="T"/> from the table, without using a key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Enumerable of all entities of type <typeparamref name="T"/>.</returns>
    public static IEnumerable<T> GetAllNoKey<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var tname = DapperHelperExtend.GetTableName(typeof(T));
        var sql = "select * from " +tname;
        var all= connection.Query<T>(sql,null,transaction,true,commandTimeout);
        return all;
    }
    /// <summary>
    /// Asynchronously gets all entities of type <typeparamref name="T"/> from the table, without using a key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="connection">Database connection.</param>
    /// <param name="transaction">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Task returning an enumerable of all entities of type <typeparamref name="T"/>.</returns>
    public static Task<IEnumerable<T>> GetAllNoKeyAsync<T>(this IDbConnection connection, IDbTransaction? transaction = null, int? commandTimeout = null) where T : class
    {
        var tname = DapperHelperExtend.GetTableName(typeof(T));
        var sql = "select * from " + tname;
        var all = connection.QueryAsync<T>(sql, null, transaction,commandTimeout: commandTimeout);
        return all;
    }

}