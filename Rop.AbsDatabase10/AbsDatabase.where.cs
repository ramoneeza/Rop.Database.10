/*
 * -----------------------------------------------------------------------------
 * <ramon@eeza.csic.es> wrote this file. As long as you retain this notice 
 * you can do whatever you want with this stuff. If we meet some day, and you 
 * think this stuff is worth it, you can buy me a coffee in return.
 * -----------------------------------------------------------------------------
 *
 * File: AbsDatabase.where.cs
 *
 * Abstract: Database methods for getting data from tables
 *
 * Copyright (c) 2024 Ramon Ordiales Plaza
 */

namespace Rop.Database
{
    public partial class AbsDatabase
    {
        /// <summary>
        /// Get some items from the database using a where clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        
        public EnumerableResult<T> GetWhere<T>(string where,object? param=null) where T:class
        {
            return UnitOfWork(conn => conn.GetWhere<T>(where,param));
        }
        /// <summary>
        /// Asynchronously get some items from the database using a where clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<EnumerableResult<T>> GetWhereAsync<T>(string where,object? param = null)where T:class
        {
            return await UnitOfWorkAsync(conn=>conn.GetWhereAsync<T>(where,param));
        }
        /// <summary>
        /// Get some items from the database using a slim where clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public EnumerableResult<T> GetWhereSlim<T>(string where,object? param = null) where T:class
        {
            return UnitOfWork(conn => conn.GetWhereSlim<T>(where, param));
        }
        /// <summary>
        /// Asynchronously get some items from the database using a slim where clause
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<EnumerableResult<T>> GetWhereSlimAsync<T>(string where,object? param = null) where T : class
        {
            return await UnitOfWorkAsync(conn=>conn.GetWhereSlimAsync<T>(where,param));
        }
    }
}
