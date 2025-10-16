using System.Diagnostics.CodeAnalysis;

namespace Rop.CycleTask;

public class CycleQueue<T> where T : class,ICycleTask
{
    private readonly PriorityQueueDateTime _queue = new();
    private readonly Lock _lock = new();
    private class PriorityQueueDateTime : PriorityQueue<CycleTaskWrapper<T>, DateTimeOffset> { }
    /// <summary>
    /// Clear the queue
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _queue.Clear();
        }
    }
    /// <summary>
    /// Enqueue a ClycleTask
    /// </summary>
    /// <param name="element"></param>
    public CycleTaskWrapper<T> Enqueue(T element)
    {
        var wrapper = new CycleTaskWrapper<T>(element);
        Enqueue(wrapper);
        return wrapper;
    }

    public void Enqueue(CycleTaskWrapper<T> element)
    {
        lock (_lock)
        {
            _queue.Enqueue(element,element.NextRun);
        }
    }

    /// <summary>
    /// Try to dequeue a CycleTask
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool TryDequeue([MaybeNullWhen(false)] out CycleTaskWrapper<T> element)
    {
        lock (_lock)
        {
            return _queue.TryDequeue(out element, out _);
        }
    }
    /// <summary>
    /// Try to dequeue a CycleTask in time
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public bool TryDequeueInTime([MaybeNullWhen(false)] out CycleTaskWrapper<T> element)
    {
        lock (_lock)
        {
            element = null;
            if (!_queue.TryPeek(out var wrapper, out _)) return false;
            if (wrapper.NextRun > DateTime.Now) return false;
            element = _queue.Dequeue();
            return true;
        }
    }
    /// <summary>
    /// Try to peek a CycleTask
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>

    public bool TryPeek([MaybeNullWhen(false)] out CycleTaskWrapper<T> element)
    {
        lock (_lock)
        {
            return _queue.TryPeek(out element, out _);
        }
    }
    /// <summary>
    /// Get the count of the queue
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _queue.Count;
            }
        }
    }

    public T? FirstOrDefault(Func<T, bool> predicate)
    {
        lock (_lock)
        {
            var item = _queue.UnorderedItems.Select(x=>x.Element).FirstOrDefault(x=>predicate(x.Item));
            return item?.Item;
        }
    }
    public bool Remove(T item)
    {
        lock (_lock)
        {
            var wrapper = _queue.UnorderedItems.Select(x=>x.Element).FirstOrDefault(x => x.Item == item);
            if (wrapper != null)
            {
                _queue.Remove(wrapper,out _,out _);
                return true;
            }
            return false;
        }
    }
}