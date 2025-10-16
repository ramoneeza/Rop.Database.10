using System.Collections;
using System.Collections.Frozen;
using System.Reflection;
using Rop.Database10.Repository;

namespace Rop.Database10.CacheRepository
{
    /// <summary>
    /// Base class for a simple SQL repository that expired after a specified time with key of type K and DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="K"></typeparam>
    public abstract class AbsSimpleCacheSqlRepositoryK<T, D, K> : IAbsSimpleSqlRepository<T, K>
        where T : class where D : class where K : notnull
    {
        public event EventHandler? OnChange;
        public event EventHandler<RecordsChangedEventArgs<K>>? OnRecordsChanged;
        public event EventHandler<RecordsChangedEventArgs<T>>? OnItemsChanged;
        public TimeSpan DefaultExpirationTime { get; }
        public TimeSpan DefaultExpirationCleanTime { get; }
        public Database Database { get; }
        protected PropertyInfo KeyPropD => KeyDescription.KeyProp;
        public KeyDescription KeyDescription { get; }
        protected PropertyInfo KeyPropT { get; }
        protected MemoryCache<K, T> Repository { get; }
        protected string SqlLoadKeys { get; }
        private readonly Lock _lockdic = new();

        // Constructor


        protected AbsSimpleCacheSqlRepositoryK(Database database, TimeSpan? defaultExpirationTime = null,
            TimeSpan? defaultcleantime = null)
        {
            Database = database.FactoryExternalDatabase<D>();
            KeyDescription = DapperHelperExtend.GetKeyDescription<D>() ??
                             throw new Exception($"Type {typeof(D)} has not key");
            KeyPropT = typeof(T).GetProperty(KeyDescription.KeyProp.Name) ??
                       throw new Exception($"Type {typeof(T)} has not key");
            DefaultExpirationTime = defaultExpirationTime ?? TimeSpan.FromMinutes(5);
            DefaultExpirationCleanTime = defaultcleantime ?? TimeSpan.FromMinutes(10);
            var eq = (Type.GetTypeCode(typeof(K)) == TypeCode.String)
                ? (IEqualityComparer<K>)StringComparer.OrdinalIgnoreCase
                : EqualityComparer<K>.Default;
            Repository = new MemoryCache<K, T>(GetTKey, DefaultExpirationTime, DefaultExpirationCleanTime, eq);
            SqlLoadKeys = $"SELECT {KeyDescription.KeyName} FROM {KeyDescription.TableName}";
        }

        private T? IntGet(K key)
        {
            var r = IntReloadSome([key]);
            if (r.IsFailedOrEmpty) return null;
            return r.Value[0];
        }

        protected abstract T Map(D item);
        public virtual K GetTKey(T item) => (K)(KeyPropT.GetValue(item) ?? throw new Exception("Can't get Key"));
        public virtual K GetDKey(D item) => (K)(KeyPropT.GetValue(item) ?? throw new Exception("Can't get Key"));


        protected EnumerableResult<K> IntReloadAllKeys()
        {
            lock (_lockdic)
            {
                var keys = Database.Query<K>(SqlLoadKeys);
                if (keys.IsFailed)
                {
                    LastError = keys.Error!;
                    return LastError;
                }

                Repository.ForceKeys(keys.Value!);
                MustReload = false;
                Initialized = true;
                return keys;
            }
        }

        protected abstract EnumerableResult<T> IntReloadSome(params IEnumerable<K> keys);
        protected abstract EnumerableResult<T> IntReloadAll();
        public bool HasError => LastError != null;
        public Error? LastError { get; protected set; }

        protected void OnMustInvokeChanges(IReadOnlyCollection<K>? changes)
        {
            RepositoryChanged(changes);
            OnChange?.Invoke(this, EventArgs.Empty);
            OnRecordsChanged?.Invoke(this, new RecordsChangedEventArgs<K>(changes));
            if (OnItemsChanged != null)
            {
                if (changes == null)
                {
                    var all = GetAll();
                    OnItemsChanged(this, new RecordsChangedEventArgs<T>(all));
                }
                else
                {
                    var some = GetSome(changes);
                    OnItemsChanged(this, new RecordsChangedEventArgs<T>(some));
                }
            }
        }

        public bool MustReload { get; private set; } = true;
        public bool Initialized { get; private set; } = false;
        protected virtual bool ReloadOnAnyChange => false;

        public void Reset(bool avoidsendchanges = false)
        {
            MustReload = true;
            IntReloadAllKeys();
        }

        public virtual void Init()
        {
            if (!Initialized) IntReloadAllKeys();
        }

        public virtual void Check()
        {
            if (MustReload) IntReloadAllKeys();
        }

        protected R LockUnitOfWork<R>(Func<R> a)
        {
            lock (_lockdic)
            {
                return a();
            }
        }

        public bool ResetIds(IEnumerable changes) => ResetIds(changes.Cast<K>().ToArray());

        public bool ResetIds(params IReadOnlyCollection<K> changes)
        {
            if (changes.Count == 0) return true;
            try
            {
                var r = LockUnitOfWork(() =>
                {
                    Repository.Expires(changes);
                    return true;
                });
                return r;
            }
            catch (Exception ex)
            {
                LastError = new ExceptionError(ex);
                return false;
            }
            finally
            {
                OnMustInvokeChanges(changes);
            }
        }

        public virtual T? Get(K key)
        {
            Check();
            lock (_lockdic)
            {
                var r = Repository.GetValueOrDefault(key);
                if (r == null) return null;
                if (!r.Expired) return r.Value!;
                var r2 = IntGet(key);
                if (r2 == null) return null;
                Repository.Add(r2);
                return r2;
            }
        }

        public virtual List<T> GetAll()
        {
            Check();
            lock (_lockdic)
            {
                var r = Repository.RefreshAll((count, keys) =>
                {
                    if (keys.Count > count * 0.75)
                    {
                        return IntReloadAll();
                    }
                    else
                    {
                        return IntReloadSome(keys);
                    }
                });
                return r.Value.ToList();
            }
        }

        public List<T> GetSome(IEnumerable<K> keys)
        {
            Check();
            lock (_lockdic)
            {
                var res = new List<T>();
                foreach (var key in keys)
                {
                    var r = Repository.GetValueOrDefault(key);
                    if (r == null) continue;
                    if (!r.Expired)
                    {
                        res.Add(r.Value!);
                        continue;
                    }

                    var r2 = IntGet(key);
                    if (r2 == null) continue;
                    Repository.Add(r2);
                    res.Add(r2);
                }

                return res;
            }
        }

        public FrozenDictionary<K, T> GetAllDictionary()
        {
            return GetAll().ToFrozenDictionary(GetTKey);
        }

        protected virtual void RepositoryChanged(IReadOnlyCollection<K>? changes)
        {
        }

        protected virtual bool InRange(T item) => true;



    }


    /// <summary>
    /// Base class for a simple SQL repository that expired after a specified time with key of type int and without DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="defaultExpirationTime"></param>
    /// <param name="defaultcleantime"></param>
    public abstract class AbsSimpleCacheSqlIntRepository<T>(
        Database database,
        TimeSpan? defaultExpirationTime = null,
        TimeSpan? defaultcleantime = null)
        : AbsSimpleCacheSqlRepositoryK<T, T, int>(database, defaultExpirationTime, defaultcleantime) where T : class
    {
        protected override T Map(T item) => item;
    }

    /// <summary>
    /// Base class for a simple SQL repository that expired after a specified time with key of type string and without DTO.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    /// <param name="defaultExpirationTime"></param>
    /// <param name="defaultcleantime"></param>
    public abstract class AbsSimpleCacheSqlRepository<T>(
        Database database,
        TimeSpan? defaultExpirationTime = null,
        TimeSpan? defaultcleantime = null)
        : AbsSimpleCacheSqlRepositoryK<T, T, string>(database, defaultExpirationTime, defaultcleantime) where T : class
    {
        protected override T Map(T item) => item;
    }
}
