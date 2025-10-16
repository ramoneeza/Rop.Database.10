using System.Diagnostics.CodeAnalysis;

namespace Rop.Database10.CacheRepository;

public class MemoryCache<K, T> where K : notnull where T : class
{
    private readonly Dictionary<K, InternalCacheItem> _cache;
    private readonly Timer _cleanupTimer;
    private readonly TimeSpan _defaultExpiration;
    private readonly Lock _lock = new();
    private readonly IEqualityComparer<K> _comparer;
    public event EventHandler<CacheCleanUpArgs<K>>? CacheCleanUp;
    private Func<T, K> _getKeyFn;
    public MemoryCache(Func<T,K> getKeyFn, TimeSpan defaultExpiration, TimeSpan cleanupInterval, IEqualityComparer<K>? comparer)
    {
        _getKeyFn= getKeyFn;
        _comparer = comparer ?? EqualityComparer<K>.Default;
        _cache = new Dictionary<K, InternalCacheItem>(comparer);
        _defaultExpiration = defaultExpiration;
        // Configuramos un temporizador para limpiar los elementos expirados periódicamente
        _cleanupTimer = new Timer(_cleanup, null, cleanupInterval, cleanupInterval);
    }
    public void Add(T value, TimeSpan? expiration = null)
    {
        var key= _getKeyFn(value);
        lock (_lock)
        {
            var expireAt = DateTime.UtcNow + (expiration ?? _defaultExpiration);
            _cache[key] = new InternalCacheItem(value, expireAt);
        }
    }
    public void AddRange(IEnumerable<T> items, TimeSpan? expiration)
    {
        lock (_lock)
        {
            var expireAt = DateTime.UtcNow + (expiration ?? _defaultExpiration);
            foreach (var value in items)
            {
                var key=_getKeyFn(value);
                _cache[key] = new InternalCacheItem(value, expireAt);
            }
        }
    }
    public void AddRange(params IEnumerable<T> items) => AddRange(items, null);
    public void AddRange(TimeSpan? expiration, params IEnumerable<T> items) => AddRange(items, expiration);
    public bool TryGetValue(K key, [MaybeNullWhen(false)] out CacheItem<T> value)
    {
        lock (_lock)
        {
            value = _cache.GetValueOrDefault(key)?.GetOrExpire(null);
        }
        if (value is null) return false;
        if (value.IsJustExpired) OnCacheCleanUp(key);
        return true;
    }

    public CacheItem<T>? GetValueOrDefault(K key)
    {
        TryGetValue(key, out var cacheitem);
        return cacheitem;
    }
    protected virtual void OnCacheCleanUp(params IReadOnlyCollection<K> keys)
    {
        if (CacheCleanUp is null || keys.Count==0) return;
        var args = new CacheCleanUpArgs<K>(keys);
        CacheCleanUp?.Invoke(this, args);
    }
    public bool Remove(K key)
    {
        bool c;
        lock (_lock)
        {
            c = _cache.Remove(key);
        }
        if (c) OnCacheCleanUp(key);
        return c;
    }
    public List<K> Remove(params IEnumerable<K> key)
    {
        var res = new List<K>();
        lock (_lock)
        {
            foreach (var k in key)
            {
                if (_cache.Remove(k)) res.Add(k);
            }
        }
        if (res.Any()) OnCacheCleanUp(res);
        return res;
    }
    public List<K> Expires(params IEnumerable<K> key)
    {
        var res = new List<K>();
        lock (_lock)
        {
            foreach (var k in key)
            {
                if (_cache[k].ForceExpiration()) res.Add(k);
            }
        }
        if (res.Any()) OnCacheCleanUp(res);
        return res;
    }
    public void Clear()
    {
        List<K> keys;
        lock (_lock)
        {
            keys = _cache.Keys.ToList();
            _cache.Clear();
        }
        OnCacheCleanUp(keys);
    }
    private void _cleanup(object? state)
    {
        List<K> expiredKeys;
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            expiredKeys = _cache.Where(kvp => kvp.Value.GetOrExpire(now).IsJustExpired).Select(kvp => kvp.Key).ToList();
        }
        OnCacheCleanUp(expiredKeys);
    }
    
    public void EnsureKeys(params IEnumerable<K> keys)
    {
        lock (_lock)
        {
            foreach (var key in keys)
            {
                if (!_cache.ContainsKey(key)) _cache[key] = InternalCacheItem.FactoryExpired();
            }
        }
    }
    public void ForceKeys(params IEnumerable<K> keys)
    {
        var newkeys = keys.ToHashSet(_comparer);
        lock (_lock)
        {
            var oldkeys= _cache.Keys.ToHashSet(_comparer);
            foreach (var key in oldkeys.Except(newkeys))
            {
                _cache.Remove(key);
            }
            foreach (var key in newkeys.Except(oldkeys))
            {
                _cache[key] = InternalCacheItem.FactoryExpired();
            }
        }
    }

    public EnumerableResult<T> RefreshAll(Func<int,IReadOnlyCollection<K>,EnumerableResult<T>> refreshfunction)
    {
        EnumerableResult<T> final;
        List<K> justexpiredKeys;
        lock (_lock)
        {
            var now= DateTime.UtcNow;
            var expireditems = _cache.Select(kvp =>(kvp.Key, kvp.Value.GetOrExpire(now))).Where(i=>i.Item2.IsExpired).ToList();
            var expiredkeys = expireditems.Select(i => i.Key).ToHashSet(_comparer);
            justexpiredKeys= expireditems.Where(i => i.Item2.IsJustExpired).Select(i => i.Key).ToList();
            var res = refreshfunction(_cache.Count,expiredkeys);
            if (res.IsFailed)
            {
                final = res.Error!;
            }
            else
            {
                foreach (var value in res.Value!)
                {
                    var key = _getKeyFn(value);
                    _cache[key] = new InternalCacheItem(value, DateTime.UtcNow + _defaultExpiration);
                    expiredkeys.Remove(key);
                }
                foreach (var expiredkey in expiredkeys)
                {
                    _cache.Remove(expiredkey);
                }
                justexpiredKeys.Clear();
                final = res;
            }
        }
        OnCacheCleanUp(justexpiredKeys);
        return final;
    }
    
    private record InternalCacheItem
    {
        private T? _value;
        public DateTime ExpireAt { get; private set; }
        public bool Expired(DateTime? now) => ExpireAt < (now ?? DateTime.UtcNow);
        public InternalCacheItem(T? value, DateTime expireAt)
        {
            _value = value;
            ExpireAt = expireAt;
        }
        public CacheItem<T> GetOrExpire(DateTime? now)
        {
            if (_value is null) return CacheItem<T>.ExpiredItem;
            if (!Expired(now)) return CacheItem<T>.NotExpired(_value!);
            if (_value is IDisposable d) d.Dispose();
            _value = null;
            return CacheItem<T>.JustExpiredItem;
        }
        public bool ForceExpiration()
        {
            if (_value == null) return false;
            ExpireAt = DateTime.MinValue;
            if (_value is IDisposable d) d.Dispose();
            _value = null;
            return true;
        }
        public static InternalCacheItem FactoryExpired() => new InternalCacheItem(null, DateTime.MinValue);

        public T? GetNoExpire()
        {
            return _value;
        }
    }
}