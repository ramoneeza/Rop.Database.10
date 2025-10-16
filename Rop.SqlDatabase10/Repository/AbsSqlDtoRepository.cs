namespace Rop.Database10.Repository;

public abstract class AbsSqlDtoRepository<T, D>(Database database, bool useslim = false, int priority = 3)
    : AbsSqlRepositoryK<T, D, string>(database, useslim, priority)
    where T : class
    where D : class
{
    
}

public abstract class AbsSqlPartialDtoRepository<T, D>(Database database, int priority = 3)
    : AbsSqlPartialRepositoryK<T, D, string>(database, priority)
    where T : class
    where D : class
{
}

