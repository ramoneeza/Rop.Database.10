using System;
using System.Diagnostics;

namespace Rop.CycleTask;

/// <summary>
/// Class to run tasks in a cycle
/// </summary>
/// <typeparam name="T">The task type stored in the cycle queue. Must implement <see cref="ICycleTask"/>.</typeparam>
public class CycleTaskService<K,T>:IDisposable where T: class,ICycleTask where K:notnull
{
    private readonly CycleQueue<T> _queue = new();
    private readonly Dictionary<K,CycleTaskWrapper<T>> _map=new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _task;
    /// <summary>
    /// Granularity of the pool in milliseconds
    /// </summary>
    public int PoolDelay { get; set; } = 1000;

    /// <summary>
    /// Attempts to add a new task to the queue with the specified key and task creation function.
    /// </summary>
    /// <param name="key">The unique key of the task to be added.</param>
    /// <param name="fntask">A function that creates an instance of the task if it does not already exist.</param>
    /// <returns>
    /// The existing task if a task with the specified key already exists; 
    /// otherwise, the newly created and added task.
    /// </returns>
    /// <remarks>
    /// If a task with the specified key already exists in the queue, this method will return the existing task 
    /// instead of creating a new one.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="key"/> or <paramref name="fntask"/> is <c>null</c>.
    /// </exception>
    public T TryAdd(K key,Func<K,T> fntask,TimeSpan? interval=null)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (fntask is null) throw new ArgumentNullException(nameof(fntask));
        var final=_map.GetValueOrDefault(key);
        if (final != null)
        {
            if (interval!=null && final.Interval>interval) final.Interval=interval.Value;
            return final.Item;
        }
        var item= fntask(key);
        final=_queue.Enqueue(item);
        _map[key]=final;
        if (_task == null)
        {
            // Start the background task if it's not already running
            _task = Task.Run(() => _runAsync(_cts.Token));
        }
        return item;
    }
    /// <summary>
    /// Last error in the queue
    /// </summary>
    public string? LastError { get; private set; }
    private async Task _runAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PoolDelay, ct);
                if (ct.IsCancellationRequested) break;
                if (!_queue.TryDequeueInTime(out var task)) continue;
                var r = task.Execute(ct);
                if (ct.IsCancellationRequested) break;
                if (r!=null)
                {
                    LastError=r;
                }
                _queue.Enqueue(task);
            }
            catch (OperationCanceledException ex)
            {
                Debug.Print(ex.Message);
                break;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                Debug.Print(ex.Message);
            }
        }
    }
    public void Dispose()
    {
        _cts.Cancel();
        if (_task != null)
        {
            try
            {
                _task.Wait();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}