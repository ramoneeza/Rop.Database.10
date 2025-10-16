namespace Rop.Database10.Repository
{
    /// <summary>
    /// Interface for a SQL repository with table dependency and versioning.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public interface ISqlRepository<T,K>:IAbsSimpleSqlRepository<T,K> where T : class where K : notnull
    {
        SqlTableDependency TableDependency { get; }
        long Version { get; }
    }
    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning with DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="K"></typeparam>

    public abstract class AbsSqlRepositoryK<T,D, K> : AbsSimpleSqlRepositoryK<T,D, K>,ISqlRepository<T,K> where T : class where D:class where K : notnull
    {
        public bool UseSlim { get; } = false;
        public SqlTableDependency TableDependency { get; private set; }
        public int ChangesPriority { get; }
        public long Version => TableDependency.TableVersion;
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
                ResetIds(e.GetKeys());
        }
        // Constructor
        protected AbsSqlRepositoryK(Database database, bool userslim = false, int changesPriority = 3) : base(database)
        {
            UseSlim = userslim;
            ChangesPriority = changesPriority;
            // ReSharper disable once VirtualMemberCallInConstructor
            TableDependency = FactoryTableDependency();
            TableDependency.OnChanged += TableDependency_OnChanged;
        }
        protected override EnumerableResult<T> IntReloadAll()
        {
            var alldto = UseSlim ? Database.GetAllSlim<D>() : Database.GetAll<D>();
            return alldto.Map(Map);
        }
        protected override EnumerableResult<T> IntReloadSome(params IEnumerable<K> keys)
        {

            var alldto = UseSlim ? Database.GetSomeSlim<K, D>(keys) : Database.GetSome<K, D>(keys);
            return alldto.Map(Map);
        }
    }
}