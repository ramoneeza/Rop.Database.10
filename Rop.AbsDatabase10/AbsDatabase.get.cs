/*
 * -----------------------------------------------------------------------------
 * <ramon@eeza.csic.es> wrote this file. As long as you retain this notice 
 * you can do whatever you want with this stuff. If we meet some day, and you 
 * think this stuff is worth it, you can buy me a coffee in return.
 * -----------------------------------------------------------------------------
 *
 * File: AbsDatabase.get.cs
 *
 * Abstract: Partial Database get methods
 *
 * Copyright (c) 2024 Ramon Ordiales Plaza
 */

namespace Rop.Database
{
    public partial class AbsDatabase
    {
        /// <summary>
        /// Get a single item from the database using a slim query
        /// </summary>
        /// <typeparam name="K">Key type used to lookup the entity.</typeparam>
        /// <typeparam name="T">Entity type to return.</typeparam>
        /// <param name="id">Key value to lookup.</param>
        /// <returns>Result with the entity found or null if not present.</returns>
        public Result<T> GetSlim<K,T>(K id) where T:class where K: notnull
        {
            return UnitOfWorkSingle(conn => conn.GetSlim<T>(id));
        }
        
        
        /// <summary>
        /// Get a single item from the database using a slim query
        /// </summary>
        /// <typeparam name="T">Entity type to return.</typeparam>
        /// <param name="id">Integer key value to lookup.</param>
        /// <returns>Result with the entity found or null if not present.</returns>
        public Result<T> GetSlim<T>(int id) where T:class
        {
            return GetSlim<int, T>(id);
        }
        /// <summary>
        /// Get a single item from the database using a slim query
        /// </summary>
        /// <typeparam name="T">Entity type to return.</typeparam>
        /// <param name="id">String key value to lookup.</param>
        /// <returns>Result with the entity found or null if not present.</returns>
        public Result<T> GetSlim<T>(string id) where T : class
        {
            return GetSlim<string, T>(id);
        }
        /// <summary>
        /// Get all items from the database using a slim query
        /// </summary>
        /// <typeparam name="T">Entity type to enumerate.</typeparam>
        /// <returns>Enumerable result with all entities obtained via the slim select.</returns>
        public EnumerableResult<T> GetAllSlim<T>() where T:class
        {
            return UnitOfWork(conn => conn.GetAllSlim<T>());
        }
        /// <summary>
        /// Retrieves all records of the specified type <typeparamref name="T"/> from the database
        /// that do not have a key associated with them.
        /// </summary>
        /// <typeparam name="T">The type of the records to retrieve. Must be a reference type.</typeparam>
        /// <returns>An <see cref="EnumerableResult{T}"/> containing all the records of type <typeparamref name="T"/>.</returns>
        public EnumerableResult<T> GetAllNoKey<T>() where T : class
        {
            return UnitOfWork(conn => conn.GetAllNoKey<T>());
        }
        /// <summary>
        /// Get all items from the database using a slim query asynchronously
        /// </summary>
        /// <typeparam name="T">Entity type to enumerate.</typeparam>
        /// <returns>Task returning an enumerable result with all entities obtained via the slim select.</returns>
        public async Task<EnumerableResult<T>> GetAllSlimAsync<T>() where T : class
        {
            return await UnitOfWorkAsync(conn => conn.GetAllSlimAsync<T>());
        }

        /// <summary>
        /// Get some items from the database using a slim query
        /// </summary>
        /// <typeparam name="K">Key type used in the ids collection.</typeparam>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">Collection of key values.</param>
        /// <returns>Enumerable result with the matching entities.</returns>
        public EnumerableResult<T> GetSomeSlim<K,T>( IEnumerable<K> ids) where T : class where K : notnull
        {
            return UnitOfWork(conn => conn.GetSomeSlim<T>(ids));
        }
        
        /// <summary>
        /// Get some items from the database using a slim query asynchronously
        /// </summary>
        /// <typeparam name="K">Key type used in the ids collection.</typeparam>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">Collection of key values.</param>
        /// <returns>Task returning an enumerable result with the matching entities.</returns>
        public async Task<EnumerableResult<T>> GetSomeSlimAsync<K,T>(IEnumerable<K> ids) where T : class where K : notnull
        {
            return await UnitOfWorkAsync(conn => conn.GetSomeSlimAsync<T>(ids));
        }
        

        /// <summary>
        /// Get some items from the database using a slim query
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">List of integer key values.</param>
        /// <returns>Enumerable result with the matching entities.</returns>

        public EnumerableResult<T> GetSomeSlim<T>( IEnumerable<int> ids) where T : class
        {
            return GetSomeSlim<int, T>(ids);
        }

        /// <summary>
        /// Get some items from the database using a slim query asynchronously
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">List of integer key values.</param>
        /// <returns>Task returning an enumerable result with the matching entities.</returns>
        public Task<EnumerableResult<T>> GetSomeSlimAsync<T>(IEnumerable<int> ids) where T : class
        {
            return GetSomeSlimAsync<int, T>(ids);
        }
        /// <summary>
        /// Get some items from the database using a slim query
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">List of string key values.</param>
        /// <returns>Enumerable result with the matching entities.</returns>

        public EnumerableResult<T> GetSomeSlim<T>(IEnumerable<string> ids) where T : class
        {
            return GetSomeSlim<string, T>(ids);
        }
        /// <summary>
        /// Get some items from the database using a slim query asynchronously
        /// </summary>
        /// <typeparam name="T">Entity type to retrieve.</typeparam>
        /// <param name="ids">List of string key values.</param>
        /// <returns>Task returning an enumerable result with the matching entities.</returns>
        public Task<EnumerableResult<T>> GetSomeSlimAsync<T>(IEnumerable<string> ids) where T : class
        {
            return GetSomeSlimAsync<string, T>(ids);
        }


        
        
    }
}
