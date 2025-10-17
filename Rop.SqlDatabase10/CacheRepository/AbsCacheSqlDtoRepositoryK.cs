using Rop.Database10.Tracking;

namespace Rop.Database10.CacheRepository
{
    /// <summary>
    /// Base class for a repository with cache that expires after a specified time with DTO and sql table dependency and int key.
    /// </summary>
    /// <typeparam name="T">Entity type (business model)</typeparam>
    /// <typeparam name="D">DTO type (database table representation)</typeparam>
    /// <param name="database">Database instance</param>
    /// <param name="changespriority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
    /// <param name="useslim">Use optimized slim queries (default: false)</param>
    /// <param name="defaultExpirationTime">Time before cache entries expire (default: 5 minutes)</param>
    public abstract class AbsCacheSqlDtoIntRepository<T, D>(
        Database database,
        ChangeTrackingPriority? changespriority = null,
        bool useslim = false,
        TimeSpan? defaultExpirationTime = null)
        : AbsCacheSqlRepositoryK<T, D, int>(database, changespriority, useslim,
            defaultExpirationTime)
        where T : class
        where D : class;

    /// <summary>
    /// Base class for a repository with cache that expires after a specified time with DTO and sql table dependency and string key.
    /// </summary>
    /// <typeparam name="T">Entity type (business model)</typeparam>
    /// <typeparam name="D">DTO type (database table representation)</typeparam>
    /// <param name="database">Database instance</param>
    /// <param name="changespriority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
    /// <param name="useslim">Use optimized slim queries (default: false)</param>
    /// <param name="defaultExpirationTime">Time before cache entries expire (default: 5 minutes)</param>
    public abstract class AbsCacheSqlDtoRepository<T, D>(
               Database database,
                      ChangeTrackingPriority? changespriority = null,
                      bool useslim = false,
                      TimeSpan? defaultExpirationTime = null)
        : AbsCacheSqlRepositoryK<T, D, string>(database, changespriority, useslim,
                       defaultExpirationTime)
        where T : class
        where D : class;
    
}