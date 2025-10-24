using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Database10.Repository;

/// <summary>
/// Internal thread-safe dictionary for repositories with some helper methods
/// </summary>
/// <typeparam name="K"></typeparam>
/// <typeparam name="D"></typeparam>
public class RepositoryDictionary<K,D> where K: notnull where D: class
{
    private readonly Dictionary<K,D> _dictionary;
    private readonly Lock _lock = new();
    public Func<D,K> FnGetKey { get; }
    public RepositoryDictionary(Func<D,K> fnGetKey)
    {
        FnGetKey = fnGetKey;
        var tc = Type.GetTypeCode(typeof(K));
        switch (tc)
        {
            case TypeCode.String:
                _dictionary = (Dictionary<K, D>)(object)(new Dictionary<string, D>(StringComparer.OrdinalIgnoreCase));
                break;
            case TypeCode.Int32:
                _dictionary = (Dictionary<K, D>)(object)(new Dictionary<string, D>(StringComparer.OrdinalIgnoreCase));
                break;
            default:
                throw new Exception($"Type {typeof(K)} not supported");
        }
    }
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _dictionary.Count;
            }
        }
    }
    public bool Remove(K key)
    {
        lock (_lock)
        {
            return _dictionary.Remove(key);
        }
    }
    public bool TryGetValue(K key,out D? value)
    {
        lock (_lock)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
    public void Clear()
    {
        lock (_lock)
        {
            _dictionary.Clear();
        }
    }
    public List<K> Keys
    {
        get
        {
            lock (_lock)
            {
                return _dictionary.Keys.ToList();
            }
        }
    }
    public List<D> Values
    {
        get
        {
            lock (_lock)
            {
                return _dictionary.Values.ToList();
            }
        }
    }
    public FrozenDictionary <K,D> ToFrozenDictionary()
    {
        lock (_lock)
        {
            return _dictionary.ToFrozenDictionary();
        }
    }
    public void ReplaceAll(IReadOnlyCollection<D> values,Func<D,bool>? inRange)
    {
        inRange ??= (_ => true);
        lock (_lock)
        {
            _dictionary.Clear();
            foreach (var value in values)
            {
                if (!inRange(value)) continue;
                var key = FnGetKey(value);
                _dictionary[key] = value;
            }
        }
    }

    public void ReplaceSome(IReadOnlyCollection<K> changes, IReadOnlyCollection<D> newvalues, Func<D, bool>? inRange)
    {
        if (changes.Count==0) return;
        inRange??=(_ => true);
        lock (_lock)
        {
            foreach (var change in changes)
            {
                _dictionary.Remove(change);
            }
            foreach (var value in newvalues)
            {
                if (!inRange(value)) continue;
                _dictionary[FnGetKey(value)] = value;
            }
        }
    }
    public D? Get(K key)
    {
        lock (_lock)
        {
            return _dictionary.GetValueOrDefault(key);
        }
    }
    public List<D> GetSome(IReadOnlyCollection<K> keys)
    {
        if (keys.Count == 0) return [];
        lock (_lock)
        {
            var list = new List<D>(keys.Count);
            foreach (var key in keys)
            {
                if (_dictionary.TryGetValue(key, out var value)) list.Add(value);
            }
            return list;
        }
    }
    public List<D> GetSome(IEnumerable<K> keys)=> GetSome(keys.ToArray());

    public List<D> Where(Func<D, bool> filter)
    {
        lock (_lock)
        {
            return _dictionary.Values.Where(filter).ToList();
        }
    }
}
