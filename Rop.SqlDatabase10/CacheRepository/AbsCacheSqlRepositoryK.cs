using Rop.Database10.Tracking;

namespace Rop.Database10.CacheRepository
{
    /// <summary>
    /// Base class for a repository with cache that expires after a specified time with DTO and sql table dependency
    /// </summary>
    /// <typeparam name="T">Entity type (business model)</typeparam>
    /// <typeparam name="D">DTO type (database table representation)</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    public abstract class AbsCacheSqlRepositoryK<T,D, K> : AbsSimpleCacheSqlRepositoryK<T,D, K> where T : class where D:class where K : notnull
    {
        public bool UseSlim { get; } = false;
        public SqlTableDependency TableDependency { get; private set; }
        public ChangeTrackingPriority ChangesPriority { get; }
        
        protected virtual SqlTableDependency FactoryTableDependency()
        {
            var td=Database.GetTableDependency(typeof(D), ChangesPriority);
            return td;
        }
        
        private void TableDependency_OnChanged(object? sender,DeltaChanges e)
        {
            ResetIds(e.GetKeys());
        }
        
        /// <summary>
        /// Constructor for cached repository with Change Tracking
        /// </summary>
        /// <param name="database">Database instance</param>
        /// <param name="changesPriority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
        /// <param name="useslim">Use optimized slim queries (default: false)</param>
        /// <param name="defaultExpirationTime">Time before cache entries expire (default: 5 minutes)</param>
        protected AbsCacheSqlRepositoryK(Database database, ChangeTrackingPriority? changesPriority = null, bool useslim = false, TimeSpan? defaultExpirationTime = null) : base(database,defaultExpirationTime)
        {
            UseSlim = useslim;
            ChangesPriority = changesPriority ?? ChangeTrackingPriority.Default;
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
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    public abstract class AbsCacheSqlRepository<T,K> : AbsCacheSqlRepositoryK<T, T, K> where T : class where K: notnull
    {
        protected override T Map(T item) =>item;
        
        /// <summary>
        /// Constructor for cached repository with Change Tracking
        /// </summary>
        /// <param name="database">Database instance</param>
        /// <param name="changesPriority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
        /// <param name="useslim">Use optimized slim queries (default: false)</param>
        /// <param name="defaultExpirationTime">Time before cache entries expire (default: 5 minutes)</param>
        protected AbsCacheSqlRepository(Database database, ChangeTrackingPriority? changesPriority = null, bool useslim = false, TimeSpan? defaultExpirationTime = null) : base(database, changesPriority, useslim, defaultExpirationTime)
        {
        }
    }
}
