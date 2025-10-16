namespace Rop.Database10.CacheRepository
{
    /// <summary>
    /// Base class for a repository with cache that expires after a specified time with DTO and sql table dependency and int key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <param name="database"></param>
    /// <param name="useslim"></param>
    /// <param name="defaultExpirationTime"></param>
    /// <param name="changespriority"></param>
    public abstract class AbsCacheSqlDtoIntRepository<T, D>(
        Database database,
        bool useslim = false,
        TimeSpan? defaultExpirationTime = null,
        int changespriority = 3)
        : AbsCacheSqlRepositoryK<T, D, int>(database, useslim,
            defaultExpirationTime, changespriority)
        where T : class
        where D : class;

    /// <summary>
    /// Base class for a repository with cache that expires after a specified time with DTO and sql table dependency and string key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <param name="database"></param>
    /// <param name="useslim"></param>
    /// <param name="defaultExpirationTime"></param>
    /// <param name="changespriority"></param>
    public abstract class AbsCacheSqlDtoRepository<T, D>(
               Database database,
                      bool useslim = false,
                      TimeSpan? defaultExpirationTime = null,
                      int changespriority = 3)
        : AbsCacheSqlRepositoryK<T, D, string>(database, useslim,
                       defaultExpirationTime, changespriority)
        where T : class
        where D : class;
    
}