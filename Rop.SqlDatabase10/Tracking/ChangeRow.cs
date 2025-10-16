using System.Collections;

namespace Rop.Database10
{
    public readonly struct ChangeRow(object id,object? id2, ChangeOperation operation)
    {
        public object Id { get; } = id;
        public object? Id2 { get; } = id2;
        public ChangeOperation Operation { get; } = operation;
    }
    public record DeltaChanges
    {
        public DeltaChanges(KeyDescription keyDescription, long version, long oldVersion, IEnumerable<ChangeRow> changes)
        {
            KeyDescription = keyDescription ?? throw new ArgumentNullException(nameof(keyDescription));
            Version= version;
            OldVersion = oldVersion;
            Changes = changes?.OrderBy(c=>c.Operation).ToList() ?? throw new ArgumentNullException(nameof(changes));
        }
        public KeyDescription KeyDescription { get; }
        public long Version { get; } 
        public long OldVersion { get; } 
        public IReadOnlyList<ChangeRow> Changes { get; }
        public IEnumerable GetKeys()=>Changes.Select(c => c.Id);

        public IEnumerable<(object, object)> GetPartialKeys()
        {
            if (KeyDescription is not PartialKeyDescription pkd) throw new InvalidOperationException("Not a partial key");
            return Changes.Select(c => (c.Id, c.Id2 ?? throw new InvalidOperationException("Not a partial key 2")));
        }
        public Type GetKeyType => KeyDescription.KeyType;
        public TypeCode GetKeyTypeCode=>Type.GetTypeCode(GetKeyType);
        public bool IsEmpty=> Changes.Count == 0;
        public bool NoNewVersion=> Version == OldVersion;
        public bool IsNewVersion=> Version > OldVersion;
        public static Result<DeltaChanges> Empty(KeyDescription kd, long lastv)
        {
            return new DeltaChanges(kd,lastv, lastv, new List<ChangeRow>());
        }
    }
    public enum ChangeOperation
    {
        Delete,
        Update,
        Insert,
        Unknown
    }
}