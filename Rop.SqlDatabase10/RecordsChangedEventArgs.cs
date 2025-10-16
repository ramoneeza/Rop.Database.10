namespace Rop.Database10
{
    /// <summary>
    /// Event arguments for records that are changed.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <param name="changes"></param>
    public class RecordsChangedEventArgs<K>(IReadOnlyCollection<K>? changes) : EventArgs
    {
        public readonly IReadOnlyCollection<K> Changes = changes??new List<K>();
        public bool Reloaded { get; }=(changes?.Count??0)==0;
    }
}