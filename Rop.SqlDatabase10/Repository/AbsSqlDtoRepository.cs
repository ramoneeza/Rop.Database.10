using Rop.Database10.PartialKeyRepository;

namespace Rop.Database10.Repository;

/// <summary>
/// Base class for SQL repositories with table dependency and versioning with specific DTO and key of type string.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
/// <param name="useslim"></param>
/// <param name="priority"></param>
public abstract class AbsSqlDtoRepository<T, D>(Database database, bool useslim = false, int priority = 3)
    : AbsSqlRepositoryK<T, D, string>(database, useslim, priority)
    where T : class
    where D : class
{
    
}
/// <summary>
/// Base class for SQL repositories with table dependency and versioning with specific DTO and key of type int.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
/// <param name="useslim"></param>
/// <param name="priority"></param>
public abstract class AbsSqlDtoIntRepository<T, D>(Database database, bool useslim = false, int priority = 3)
    : AbsSqlRepositoryK<T, D, int>(database, useslim, priority)
    where T : class
    where D : class
{
}


