namespace Rop.Database10.Repository;

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
public abstract class AbsSimpleSqlPartialIntRepository<T,D>(Database database)
    : AbsSimpleSqlPartialRepositoryK<T,D, int>(database) where T : class where D: class
{
}
public abstract class AbsSimpleSqlPartialRepository<T,D>(Database database)
    : AbsSimpleSqlPartialRepositoryK<T,D,string>(database) where T : class where D:class
{
}
public abstract class AbsSimpleSqlPartialRepository<T>(Database database)
    : AbsSimpleSqlPartialRepository<T, T>(database) where T : class
{
}
public abstract class AbsSimpleSqlPartialIntRepository<T>(Database database)
    : AbsSimpleSqlPartialIntRepository<T, T>(database) where T : class
{
}
