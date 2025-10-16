using System.Collections;
using System.Collections.Frozen;
using System.Reflection;

namespace Rop.Database10.Repository
{
    /// <summary>
    /// Interface for a simple SQL repository.
    /// </summary>
    public interface IAbsSimpleSqlRepository
    {
        event EventHandler? OnChange;
        Database Database { get; }
        KeyDescription KeyDescription { get; }
        bool HasError { get; }
        Error? LastError { get; }
        bool MustReload { get; }
        bool Initialized { get; }
        void Init();
        void Check();
        void Reset(bool avoidsendchanges = false);
    }
    /// <summary>
    /// Interface for a simple SQL repository with specific types.
    /// </summary>
    /// <typeparam name="T">Entity Type</typeparam>
    /// <typeparam name="K">Key Type</typeparam>
    public interface IAbsSimpleSqlRepository<T,K>:IAbsSimpleSqlRepository where T : class where K : notnull
    {
        event EventHandler<RecordsChangedEventArgs<K>>? OnRecordsChanged;
        event EventHandler<RecordsChangedEventArgs<T>>? OnItemsChanged;

        K GetTKey(T item);
        bool ResetIds(params IReadOnlyCollection<K> changes);
        T? Get(K key);
        List<T> GetAll();
        List<T> GetSome(IEnumerable<K> keys);
        FrozenDictionary<K, T> GetAllDictionary();
    }
    /// <summary>
    /// Base abstract class for a simple SQL repository with specific entity type, dto type and key type.
    /// </summary>
    /// <typeparam name="T">Entity Type</typeparam>
    /// <typeparam name="D">Dto type as in database table with dapper decorators</typeparam>
    /// <typeparam name="K">Key type</typeparam>
    public abstract class AbsSimpleSqlRepositoryK<T,D, K>:IAbsSimpleSqlRepository<T,K> where T : class where D:class where K : notnull
    {
        public event EventHandler? OnChange;
        public event EventHandler<RecordsChangedEventArgs<K>>? OnRecordsChanged;
        public event EventHandler<RecordsChangedEventArgs<T>>? OnItemsChanged;
        public Database Database { get; }
        public virtual KeyDescription KeyDescription { get; }
        protected PropertyCache KeyPropD => KeyDescription.KeyPropCache;
        protected PropertyCache KeyPropT { get; }
        protected abstract T Map(D item);
        public virtual K GetTKey(T item)=> (K)(KeyPropT.Getter(item)??throw new Exception("Can't get Key"));
        public virtual K GetDKey(D item)=> (K)(KeyDescription.GetKeyValue(item)??throw new Exception("Can't get Key"));
        protected abstract EnumerableResult<T> IntReloadAll();
        protected abstract EnumerableResult<T> IntReloadSome(params IEnumerable<K> keys);
        protected RepositoryDictionary<K, T> Repository { get; }
        private readonly Lock _lockreload = new();
        public bool HasError => LastError != null;
        public Error? LastError { get; protected set; }
        protected void OnMustInvokeChanges(IReadOnlyCollection<K>? changes)
        {
            RepositoryChanged(changes);
            OnChange?.Invoke(this, EventArgs.Empty);
            OnRecordsChanged?.Invoke(this, new RecordsChangedEventArgs<K>(changes));
            if (changes == null)
            {
                var all = GetAll();
                OnItemsChanged?.Invoke(this, new RecordsChangedEventArgs<T>(all));
            }
            else
            {
                var some = GetSome(changes);
                OnItemsChanged?.Invoke(this, new RecordsChangedEventArgs<T>(some));
            }
        }
        public bool MustReload { get; private set; } = true;
        public bool Initialized { get; private set; } = false;
        protected virtual bool ReloadOnAnyChange => false;
        public void Reset(bool avoidsendchanges = false)
        {
            lock (_lockreload)
            {
                MustReload = true;
                ReloadAll(avoidsendchanges);
            }
        }
        public virtual void Init()
        {
            lock (_lockreload)
            {

                if (!Initialized)
                    ReloadAll(true);
            }
        }
        public virtual void Check()
        {
            lock (_lockreload)
            {
                if (MustReload) ReloadAll();
            }
        }
        protected VoidResult ReloadAll(bool avoidsendchanges = false)
        {
            LastError = null;
            var rall=IntReloadAll();
            if (rall.IsFailed)
            {
                LastError = rall.Error;
                return rall;
            }
            Repository.ReplaceAll(rall.Value,InRange);
            MustReload = false;
            Initialized = true;
            if (!avoidsendchanges) OnMustInvokeChanges(null);
            return VoidResult.Ok;
        }
        public bool ResetIds(IEnumerable changes) => ResetIds(changes.Cast<K>().ToArray());
        public bool ResetIds(params IReadOnlyCollection<K> changes)
        {
            if (changes.Count == 0) return true;
            try
            {
                var rreload = IntReloadSome(changes);
                if (rreload.IsFailed)
                {
                    LastError = rreload.Error;
                    return false;
                }
                Repository.ReplaceSome(changes,rreload.Value,InRange);
                OnMustInvokeChanges(changes);
                return true;
            }
            catch (Exception ex)
            {
                LastError = new ExceptionError(ex);
                return false;
            }
        }

        public virtual T? Get(K key)
        {
            Check();
            if (key is int and <= 0) return null;
            if (key is string s && string.IsNullOrEmpty(s)) return null;
            return Repository.Get(key);
        }
        // Constructor
        protected AbsSimpleSqlRepositoryK(Database database):this(database, DapperHelperExtend.GetKeyDescription<D>())
        {
        }
        protected AbsSimpleSqlRepositoryK(Database database,KeyDescription keydescription)
        {
            Database = database.FactoryExternalDatabase<D>();
            KeyDescription = keydescription;
            var keyPropT=typeof(T).GetProperty(keydescription.KeyProp.Name) ?? throw new Exception($"Type {typeof(T)} has not key");
            KeyPropT = new PropertyCache(keyPropT);
            // ReSharper disable once VirtualMemberCallInConstructor
            Repository = new RepositoryDictionary<K, T>(GetTKey);
        }
        
        public virtual List<T> GetAll()
        {
            Check();
            return Repository.Values;
        }
        public virtual List<T> GetSome(IEnumerable<K> keys)
        {
            Check();
            return Repository.GetSome(keys.ToArray());
        }
        public FrozenDictionary<K, T> GetAllDictionary()
        {
            Check();
            return Repository.ToFrozenDictionary();
        }
        public List<T> GetFiltered(Func<T, bool> filter)
        {
            Check();
            return Repository.Where(filter);
        }
        protected virtual void RepositoryChanged(IReadOnlyCollection<K>? changes)
        {
        }
        protected virtual bool InRange(T item) => true;
    }

    /// <summary>
    /// Base abstract class for a simple SQL repository with specific entity type and dto type, using int as key type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSimpleSqlIntRepository<T,D>(Database database)
        : AbsSimpleSqlRepositoryK<T,D, int>(database) where T : class where D: class
    {
    }
    /// <summary>
    /// Base abstract class for a simple SQL repository with specific entity type and dto type, using string as key type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSimpleSqlRepository<T,D>(Database database)
        : AbsSimpleSqlRepositoryK<T,D, string>(database) where T : class where D : class
    {
    }
    /// <summary>
    /// Base abstract class for a simple SQL repository with specific entity type, using string as key type without specific DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSimpleSqlRepository<T>(Database database)
        : AbsSimpleSqlRepository<T, T>(database) where T : class
    {
        protected override T Map(T item)=> item;
    }
    /// <summary>
    /// Base abstract class for a simple SQL repository with specific entity type, using int as key type without specific DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSimpleSqlIntRepository<T>(Database database)
        : AbsSimpleSqlIntRepository<T, T>(database) where T : class
    {
        protected override T Map(T item) => item;
    }
    
}