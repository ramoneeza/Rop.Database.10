using Rop.Database10.Repository;

namespace Rop.Database10.PartialKeyRepository;


/// <summary>
/// Aggregate class for a block of items with the same first level key
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="T"></typeparam>
public class PartialBlock<K,T> where K : notnull where T : class
{
    public K Key { get; }
    public IReadOnlyList<T> Block { get; }
    public PartialBlock(K key, IEnumerable<T> block)
    {
        Key = key;
        Block = block.ToArray();
    }
}
/// <summary>
/// Base class for a simple SQL repository with partial key support and DTO.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <typeparam name="K"></typeparam>
public abstract class AbsSimpleSqlPartialRepositoryK<T,D,K>:AbsSimpleSqlRepositoryK<PartialBlock<K,T>,PartialBlock<K,D>,K> where T:class where D:class where K:notnull
{
    public override PartialKeyDescription KeyDescription =>(PartialKeyDescription)base.KeyDescription;
    public virtual K GetKey(D item) => (K)KeyDescription.GetKeyValue(item);
    public abstract K GetKey(T item);
    public override K GetTKey(PartialBlock<K, T> item) => item.Key;
    public override K GetDKey(PartialBlock<K, D> item) => item.Key;

    protected abstract T Map(D item);
    
    protected override PartialBlock<K, T> Map(PartialBlock<K, D> item)
    {
        var lst=item.Block.Select(Map);
        return new PartialBlock<K, T>(item.Key, lst);
    }

    protected abstract EnumerableResult<D> IntReloadAllPartial();

    protected override EnumerableResult<PartialBlock<K, T>> IntReloadAll()
    {
        var rall= IntReloadAllPartial();
        if (rall.IsFailed) return rall.Error!;
        var gr=rall.Value.GroupBy(GetKey).Select(g =>Map(new PartialBlock<K, D>(g.Key, g.ToList())));
        return gr.ToList();
    }
    protected abstract EnumerableResult<D> IntReloadSomePartial(params IEnumerable<K> keys);
    protected override EnumerableResult<PartialBlock<K, T>> IntReloadSome(params IEnumerable<K> keys)
    {
        var rall= IntReloadSomePartial(keys);
        if (rall.IsFailed) return rall.Error!;
        var gr=rall.Value.GroupBy(GetKey).Select(g => Map(new PartialBlock<K, D>(g.Key, g.ToList())));
        return gr.ToList();
    }

    // Constructor
    protected AbsSimpleSqlPartialRepositoryK(Database database):base(database,DapperHelperExtend.GetPartialKeyDescription(typeof(D)))
    {
    }
}

/// <summary>
/// Base class for a simple SQL repository with partial key support and key of type int and DTO.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
public abstract class AbsSimpleSqlPartialIntRepository<T,D>(Database database)
    : AbsSimpleSqlPartialRepositoryK<T,D, int>(database) where T : class where D: class
{
}
/// <summary>
/// Base class for a simple SQL repository with partial key support and key of type string and DTO.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
public abstract class AbsSimpleSqlPartialRepository<T,D>(Database database)
    : AbsSimpleSqlPartialRepositoryK<T,D,string>(database) where T : class where D:class
{
}
/// <summary>
/// Base class for a simple SQL repository with partial key support without specific DTO and key of type string.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="database"></param>
public abstract class AbsSimpleSqlPartialRepository<T>(Database database)
    : AbsSimpleSqlPartialRepository<T, T>(database) where T : class
{
}
/// <summary>
/// Base class for a simple SQL repository with partial key support without specific DTO and key of type int.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="database"></param>
public abstract class AbsSimpleSqlPartialIntRepository<T>(Database database)
    : AbsSimpleSqlPartialIntRepository<T, T>(database) where T : class
{
}
/// <summary>
/// Base class for a SQL repository with partial key support and DTO and Change Tracking.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <typeparam name="K"></typeparam>
public abstract class AbsSqlPartialRepositoryK<T, D, K> : AbsSimpleSqlPartialRepositoryK<T, D, K> where T : class where D : class where K : notnull
{
    public SqlTableDependency TableDependency { get; private set; }
    public int ChangesPriority { get; }
    public abstract K CombineKeys(object? key1, object? key2);
    protected virtual SqlTableDependency FactoryTableDependency()
    {
        return Database.GetTableDependency(typeof(D), ChangesPriority);
    }
    private void TableDependency_OnChanged(object? sender, DeltaChanges e)
    {
        if (ReloadOnAnyChange)
        {
            Reset();
        }
        else
        {
            var keys = e.GetPartialKeys().Select(t => CombineKeys(t.Item1, t.Item2)).ToArray();
            ResetIds(keys);
        }
    }
    // Constructor
    protected AbsSqlPartialRepositoryK(Database database, int changesPriority = 3) : base(database)
    {
        ChangesPriority = changesPriority;
        // ReSharper disable once VirtualMemberCallInConstructor
        TableDependency = FactoryTableDependency();
        TableDependency.OnChanged += TableDependency_OnChanged;
    }

    protected override EnumerableResult<D> IntReloadAllPartial()
    {
        return Database.GetAllNoKey<D>();
    }

    protected override EnumerableResult<D> IntReloadSomePartial(params IEnumerable<K> keys)
    {
        return Database.GetSome<K, D>(keys);
    }

}
/// <summary>
/// Base class for a SQL repository with partial key support and DTO and Change Tracking with key of type int.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
/// <param name="priority"></param>

public abstract class AbsSqlPartialDtoIntRepository<T, D>(Database database, int priority = 3)
    : AbsSqlPartialRepositoryK<T, D, int>(database, priority)
    where T : class
    where D : class
{
}
/// <summary>
/// Base class for a SQL repository with partial key support and DTO and Change Tracking with key of type string.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="D"></typeparam>
/// <param name="database"></param>
/// <param name="priority"></param>

public abstract class AbsSqlPartialDtoRepository<T, D>(Database database, int priority = 3)
    : AbsSqlPartialRepositoryK<T, D, string>(database, priority)
    where T : class
    where D : class
{
}