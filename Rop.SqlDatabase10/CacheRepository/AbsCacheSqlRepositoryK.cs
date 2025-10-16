namespace Rop.Database10.CacheRepository
{
    /// <summary>
    /// Base class for a repository with cache that expires fter a specified time whith DTO and sql table dependency
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="K"></typeparam>
    public abstract class AbsCacheSqlRepositoryK<T,D, K> : AbsSimpleCacheSqlRepositoryK<T,D, K> where T : class where D:class where K : notnull
    {
        public bool UseSlim { get; } = false;
        public SqlTableDependency TableDependency { get; private set; }
        public int ChangesPriority { get; }
        protected virtual SqlTableDependency FactoryTableDependency()
        {
            var td=Database.GetTableDependency(typeof(D), ChangesPriority);
            return td;
        }
        private void TableDependency_OnChanged(object? sender,DeltaChanges e)
        {
            ResetIds(e.GetKeys());
        }
        // Constructor
        protected AbsCacheSqlRepositoryK(Database database, bool userslim = false,TimeSpan? defaultExpirationTime=null, int changesPriority = 3) : base(database,defaultExpirationTime)
        {
            UseSlim = userslim;
            ChangesPriority = changesPriority;
            // ReSharper disable once VirtualMemberCallInConstructor
            TableDependency = FactoryTableDependency();
            TableDependency.OnChanged += TableDependency_OnChanged;
        }
        protected override EnumerableResult<T> IntReloadSome(IEnumerable<K> keys)
        {

            var alldto = UseSlim ? Database.GetSomeSlim<K, D>(keys) : Database.GetSome<K, D>(keys);
            return alldto.Map(Map);
        }
    }
    /// <summary>
    /// Base class for a repository with cache that expires after a specified time without specific DTO and sql table dependency
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    public abstract class AbsCacheSqlRepository<T,K> : AbsCacheSqlRepositoryK<T, T, K> where T : class where K: notnull
    {
        protected override T Map(T item) =>item;
        protected AbsCacheSqlRepository(Database database, bool userslim = false, TimeSpan? defaultExpirationTime = null, int changesPriority = 3) : base(database, userslim, defaultExpirationTime, changesPriority)
        {
        }
    }
}
