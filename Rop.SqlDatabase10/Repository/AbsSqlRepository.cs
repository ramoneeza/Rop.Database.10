using Rop.Database10.Tracking;

namespace Rop.Database10.Repository
{
    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning without specific DTO and key of type string.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="database">Database instance</param>
    /// <param name="priority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
    public abstract class AbsSqlRepository<T>(Database database, ChangeTrackingPriority? priority = null) 
        : AbsSqlDtoRepository<T, T>(database, priority)
        where T : class
    {
        public virtual string GetKey(T item) => base.GetTKey(item);
        public override string GetTKey(T item) => GetKey(item);
        public override string GetDKey(T item) => GetKey(item);
        protected override T Map(T item) => item;
    }

    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning without specific DTO and key of type int.
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="database">Database instance</param>
    /// <param name="priority">Change tracking priority. If null, uses Default (Medium - check every 16 seconds)</param>
    public abstract class AbsSqlIntRepository<T>(Database database, ChangeTrackingPriority? priority = null) 
        : AbsSqlDtoIntRepository<T, T>(database, priority)
        where T : class
    {
        public virtual int GetKey(T item) => base.GetTKey(item);
        public override int GetTKey(T item) => GetKey(item);
        public override int GetDKey(T item) => GetKey(item);
        protected override T Map(T item) => item;
    }
}