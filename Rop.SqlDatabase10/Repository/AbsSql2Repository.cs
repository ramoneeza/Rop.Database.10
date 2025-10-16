namespace Rop.Database10.Repository;

public abstract class AbsSqlPartialRepositoryK<T,D,K> : AbsSimpleSqlPartialRepositoryK<T,D,K> where T : class where D:class where K:notnull
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
        return Database.GetSome<K,D>(keys);
    }

}
