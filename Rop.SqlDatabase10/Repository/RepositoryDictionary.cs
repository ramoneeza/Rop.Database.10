using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Database10.Repository;

/// <summary>
/// Internal thread-safe dictionary for repositories with helper methods.
/// </summary>
/// <typeparam name="K">Key type (string or int).</typeparam>
/// <typeparam name="D">Entity type stored in the dictionary.</typeparam>
public class RepositoryDictionary<K,D> where K: notnull where D: class
{
    private readonly Dictionary<K,D> _dictionary;
    private readonly Lock _lock = new();
    /// <summary>
    /// Gets the function used to extract a key from an entity.
    /// </summary>
    public Func<D,K> FnGetKey { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryDictionary{K,D}"/> class.
    /// </summary>
    /// <param name="fnGetKey">Function to extract the key from an entity.</param>
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
                _dictionary = (Dictionary<K, D>)(object)(new Dictionary<int, D>());
                break;
            default:
                throw new Exception($"Type {typeof(K)} not supported");
        }
    }

    /// <summary>
    /// Executes a function under the dictionary lock and returns a value.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    /// <param name="function">Function to execute.</param>
    /// <returns>Function result.</returns>
    protected T LockUnitOfWork<T>(Func<Dictionary<K, D>,T> function)
    {
        lock (_lock)
        {
            return function(_dictionary);
        }
    }
    /// <summary>
    /// Executes an action under the dictionary lock.
    /// </summary>
    /// <param name="function">Action to execute.</param>
    protected void LockUnitOfWork(Action<Dictionary<K,D>> function)
    {
        lock (_lock)
        {
            function(_dictionary);
        }
    }

    /// <summary>
    /// Gets the number of items in the dictionary.
    /// </summary>
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
    /// <summary>
    /// Removes a key from the dictionary.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns>True if the key was removed.</returns>
    public bool Remove(K key)
    {
        lock (_lock)
        {
            return IntRemove(key, _dictionary);
        }
    }
    /// <summary>
    /// Tries to get a value by key.
    /// </summary>
    /// <param name="key">Key to lookup.</param>
    /// <param name="value">Value if found.</param>
    /// <returns>True if found.</returns>
    public bool TryGetValue(K key,out D? value)
    {
        lock (_lock)
        {
            return _dictionary.TryGetValue(key, out value);
        }
    }
    /// <summary>
    /// Clears all entries from the dictionary.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            IntClear();
        }
    }
    /// <summary>
    /// Gets the list of keys.
    /// </summary>
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
    /// <summary>
    /// Gets the list of values.
    /// </summary>
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
    /// <summary>
    /// Creates a frozen snapshot of the dictionary.
    /// </summary>
    /// <returns>Frozen dictionary snapshot.</returns>
    public FrozenDictionary <K,D> ToFrozenDictionary()
    {
        lock (_lock)
        {
            return _dictionary.ToFrozenDictionary();
        }
    }

    /// <summary>
    /// Removes a key without locking. Callers must ensure synchronization.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <param name="dictionary">Dictionary to operate on.</param>
    /// <returns>True if removed.</returns>
    protected virtual bool IntRemove(K key, Dictionary<K, D> dictionary)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        return dictionary.Remove(key);
    }
    /// <summary>
    /// Adds or replaces a value without locking. Callers must ensure synchronization.
    /// </summary>
    /// <param name="key">Key to add.</param>
    /// <param name="value">Value to add.</param>
    protected virtual void IntAdd(K key,D value)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        _dictionary[key] = value;
    }

    /// <summary>
    /// Clears entries without locking. Callers must ensure synchronization.
    /// </summary>
    protected virtual void IntClear()
    {
        _dictionary.Clear();
    }

    /// <summary>
    /// Replaces all entries using the provided values.
    /// </summary>
    /// <param name="values">Values to load.</param>
    /// <param name="inRange">Optional filter predicate.</param>
    public virtual void ReplaceAll(IReadOnlyCollection<D> values,Func<D,bool>? inRange)
    {
        inRange ??= (_ => true);
        lock (_lock)
        {
            IntClear();
            foreach (var value in values)
            {
                if (!inRange(value)) continue;
                var key = FnGetKey(value);
                IntAdd(key, value);
            }
        }
    }

    /// <summary>
    /// Replaces a subset of entries for the specified keys.
    /// </summary>
    /// <param name="changes">Keys to replace.</param>
    /// <param name="newvalues">New values for those keys.</param>
    /// <param name="inRange">Optional filter predicate.</param>
    public virtual void ReplaceSome(IReadOnlyCollection<K> changes, IReadOnlyCollection<D> newvalues, Func<D, bool>? inRange)
    {
        if (changes.Count==0) return;
        inRange??=(_ => true);
        lock (_lock)
        {
            foreach (var change in changes)
            {
                IntRemove(change,_dictionary);
            }
            foreach (var value in newvalues)
            {
                if (!inRange(value)) continue;
                IntAdd(FnGetKey(value), value);
            }
        }
    }
    /// <summary>
    /// Gets a value by key, or null if not found.
    /// </summary>
    /// <param name="key">Key to lookup.</param>
    /// <returns>Value or null.</returns>
    public D? Get(K key)
    {
        lock (_lock)
        {
            return _dictionary.GetValueOrDefault(key);
        }
    }
    /// <summary>
    /// Gets a list of values for the specified keys.
    /// </summary>
    /// <param name="keys">Keys to lookup.</param>
    /// <returns>List of found values.</returns>
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
    /// <summary>
    /// Gets a list of values for the specified keys.
    /// </summary>
    /// <param name="keys">Keys to lookup.</param>
    /// <returns>List of found values.</returns>
    public List<D> GetSome(IEnumerable<K> keys)=> GetSome(keys.ToArray());

    /// <summary>
    /// Filters values using a predicate.
    /// </summary>
    /// <param name="filter">Predicate to apply.</param>
    /// <returns>List of matching values.</returns>
    public List<D> Where(Func<D, bool> filter)
    {
        lock (_lock)
        {
            return _dictionary.Values.Where(filter).ToList();
        }
    }
}
