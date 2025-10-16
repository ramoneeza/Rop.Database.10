namespace Rop.Database10.CacheRepository;

public class CacheCleanUpArgs<K> : EventArgs where K : notnull
{
    public IReadOnlyCollection<K> CleanUpKeys { get; }
    public CacheCleanUpArgs(IReadOnlyCollection<K> keys)
    {
        CleanUpKeys = keys;
    }
}