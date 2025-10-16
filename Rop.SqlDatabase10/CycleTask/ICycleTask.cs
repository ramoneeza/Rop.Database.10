namespace Rop.Database10.CycleTask;

public interface ICycleTask
{
    TimeSpan Interval { get; }
    string? PayLoad();
}