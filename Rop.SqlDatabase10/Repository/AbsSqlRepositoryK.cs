using Rop.Database10.Tracking;

namespace Rop.Database10.Repository
{
    /// <summary>
    /// Interface for a SQL repository with table dependency and versioning.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    public interface ISqlRepository<T,K>:IAbsSimpleSqlRepository<T,K> where T : class where K : notnull
    {
        SqlTableDependency TableDependency { get; }
        long Version { get; }
    }
    
    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning with DTO.
    /// </summary>
    /// <typeparam name="T">Entity type (business model)</typeparam>
    /// <typeparam name="D">DTO type (database table representation)</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    public abstract class AbsSqlRepositoryK<T,D, K> : AbsSimpleSqlRepositoryK<T,D, K>,ISqlRepository<T,K> where T : class where D:class where K : notnull
    {
        public bool UseSlim { get; } = false;
        public SqlTableDependency TableDependency { get; private set; }
        public ChangeTrackingPriority ChangesPriority { get; }
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
        
        /// <summary>
        /// Constructor for SQL repository with Change Tracking
        /// </summary>
        /// <param name="database">Database instance</param>
        /// <param name="changesPriority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
        /// <param name="useslim">Use optimized slim queries (default: false)</param>
        protected AbsSqlRepositoryK(Database database, ChangeTrackingPriority? changesPriority = null, bool useslim = false) : base(database)
        {
            UseSlim = useslim;
            ChangesPriority = changesPriority ?? ChangeTrackingPriority.Default;
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