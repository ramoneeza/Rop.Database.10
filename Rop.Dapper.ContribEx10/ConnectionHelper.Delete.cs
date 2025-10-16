using System.Data;
using Dapper;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides extension methods for deleting entities by key or partial key using Dapper.
/// </summary>
public static partial class ConnectionHelper
{
    /// <summary>
    /// Deletes an entity of type <typeparamref name="T"/> by its key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>True if one or more rows were deleted, false otherwise.</returns>
    public static bool DeleteByKey<T>(this IDbConnection conn, dynamic id, IDbTransaction? tr = null, int? commandTimeout = null)
    {
        var sql = DapperHelperExtend.DeleteByKeyCache(typeof(T));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var n = conn.Execute(sql, dynParams, tr, commandTimeout);
        return n > 0;
    }
    /// <summary>
    /// Deletes entities of type <typeparamref name="T"/> by a partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Partial key value.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Number of rows deleted.</returns>
    public static int DeleteByPartialKey<T>(this IDbConnection conn, dynamic id, IDbTransaction? tr = null, int? commandTimeout = null)
    {
        var sql = DapperHelperExtend.DeleteByPartialKeyCache(typeof(T));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var n = conn.Execute(sql, dynParams, tr, commandTimeout);
        return n;
    }
    // Async 
    /// <summary>
    /// Asynchronously deletes an entity of type <typeparamref name="T"/> by its key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Key value.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>True if one or more rows were deleted, false otherwise.</returns>
    public static async Task<bool> DeleteByKeyAsync<T>(this IDbConnection conn, dynamic id, IDbTransaction? tr = null, int? commandTimeout = null)
    {
        var sql = DapperHelperExtend.DeleteByKeyCache(typeof(T));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var n = await conn.ExecuteAsync(sql, dynParams, tr, commandTimeout);
        return n > 0;
    }
    /// <summary>
    /// Asynchronously deletes entities of type <typeparamref name="T"/> by a partial key.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="conn">Database connection.</param>
    /// <param name="id">Partial key value.</param>
    /// <param name="tr">Optional transaction.</param>
    /// <param name="commandTimeout">Optional command timeout.</param>
    /// <returns>Number of rows deleted.</returns>
    public static async Task<int> DeleteByPartialKeyAsync<T>(this IDbConnection conn, dynamic id, IDbTransaction? tr = null, int? commandTimeout = null)
    {
        var sql = DapperHelperExtend.DeleteByPartialKeyCache(typeof(T));
        var dynParams = new DynamicParameters();
        dynParams.Add("@id", id);
        var n = await conn.ExecuteAsync(sql, dynParams, tr, commandTimeout);
        return n;
    }
}