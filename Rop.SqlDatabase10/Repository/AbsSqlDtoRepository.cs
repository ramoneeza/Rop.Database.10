using Rop.Database10.PartialKeyRepository;
using Rop.Database10.Tracking;

namespace Rop.Database10.Repository;

/// <summary>
/// Base class for SQL repositories with table dependency and versioning with specific DTO and key of type string.
/// </summary>
/// <typeparam name="T">Entity type (business model)</typeparam>
/// <typeparam name="D">DTO type (database table representation)</typeparam>
/// <param name="database">Database instance</param>
/// <param name="priority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
/// <param name="useslim">Use optimized slim queries (default: false)</param>
public abstract class AbsSqlDtoRepository<T, D>(Database database, ChangeTrackingPriority? priority = null, bool useslim = false)
    : AbsSqlRepositoryK<T, D, string>(database, priority, useslim)
    where T : class
    where D : class
{
    
}
/// <summary>
/// Base class for SQL repositories with table dependency and versioning with specific DTO and key of type int.
/// </summary>
/// <typeparam name="T">Entity type (business model)</typeparam>
/// <typeparam name="D">DTO type (database table representation)</typeparam>
/// <param name="database">Database instance</param>
/// <param name="priority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
/// <param name="useslim">Use optimized slim queries (default: false)</param>
public abstract class AbsSqlDtoIntRepository<T, D>(Database database, ChangeTrackingPriority? priority = null, bool useslim = false)
    : AbsSqlRepositoryK<T, D, int>(database, priority, useslim)
    where T : class
    where D : class
{
}


