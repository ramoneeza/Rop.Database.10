namespace Rop.CycleTask;

public interface ICycleTask
{
    TimeSpan Interval { get; }
    string? PayLoad();
}