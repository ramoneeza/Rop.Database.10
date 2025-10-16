namespace Rop.Database10.CacheRepository
{
  
    public abstract class AbsCacheSqlDtoIntRepository<T, D>(
        Database database,
        bool useslim = false,
        TimeSpan? defaultExpirationTime = null,
        int changespriority = 3)
        : AbsCacheSqlRepositoryK<T, D, int>(database, useslim,
            defaultExpirationTime, changespriority)
        where T : class
        where D : class;
    public abstract class AbsCacheSqlDtopRepository<T, D>(
               Database database,
                      bool useslim = false,
                      TimeSpan? defaultExpirationTime = null,
                      int changespriority = 3)
        : AbsCacheSqlRepositoryK<T, D, string>(database, useslim,
                       defaultExpirationTime, changespriority)
        where T : class
        where D : class;
    
}