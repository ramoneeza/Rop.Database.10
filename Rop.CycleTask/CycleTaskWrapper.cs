namespace Rop.CycleTask;

public class CycleTaskWrapper<T> where T: ICycleTask
{
    public T Item { get; set; }
    public TimeSpan Interval { get; set; }
    public DateTimeOffset LastRun { get; private set; } = DateTimeOffset.MinValue;
    public DateTimeOffset NextRun { get; private set; } = DateTimeOffset.Now;
    public TimeSpan Delay { get; private set; }
    
    public CycleTaskWrapper(T item)
    {
        Item=item;
        Interval=item.Interval;
    }
    public string? Execute(CancellationToken? ct = null)
    {
        if (ct?.IsCancellationRequested ?? false) return null;
        var now = DateTimeOffset.Now;
        if (now < NextRun) return null;
        LastRun = now;
        try
        {
            return Item.PayLoad();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        finally
        {
            var end = DateTime.Now;
            Delay = end - now;
            NextRun = end + Interval;
        }
    }
}