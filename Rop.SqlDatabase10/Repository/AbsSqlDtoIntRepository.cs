namespace Rop.Database10.Repository;

public abstract class AbsSqlDtoIntRepository<T, D>(Database database, bool useslim = false, int priority = 3)
    : AbsSqlRepositoryK<T, D, int>(database, useslim, priority)
    where T : class
    where D : class
{
}
public abstract class AbsSqlPartialDtoIntRepository<T, D>(Database database, int priority = 3)
    : AbsSqlPartialRepositoryK<T, D, int>(database, priority)
    where T : class
    where D : class
{
}