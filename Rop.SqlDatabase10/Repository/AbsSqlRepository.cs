namespace Rop.Database10.Repository
{
    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning whithout specific DTO and key of type string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSqlRepository<T>(Database database) : AbsSqlDtoRepository<T,T>(database)
        where T : class
    {
        public virtual string GetKey(T item) => base.GetTKey(item);
        public override string GetTKey(T item) => GetKey(item);
        public override string GetDKey(T item) => GetKey(item);
        protected override T Map(T item)=> item;
    }
    /// <summary>
    /// Base class for SQL repositories with table dependency and versioning whithout specific DTO and key of type int.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="database"></param>
    public abstract class AbsSqlIntRepository<T>(Database database) : AbsSqlDtoIntRepository<T, T>(database)
        where T : class
    {
        public virtual  int GetKey(T item) => base.GetTKey(item);
        public override int GetTKey(T item) => GetKey(item);
        public override int GetDKey(T item) => GetKey(item);
        protected override T Map(T item)=> item;
    }

}