namespace Rop.Database10.CacheRepository;

public record CacheItem<T>(bool Expired, bool ExpiredNow, T? Value)
{
    public bool IsExpired => Expired;
    public bool IsJustExpired => ExpiredNow;
    public static CacheItem<T> JustExpiredItem => new(true, true, default);
    public static CacheItem<T> ExpiredItem => new(true, false, default);
    public static CacheItem<T> NotExpired(T value) => new(false, false, value);
}