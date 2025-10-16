namespace Rop.Database10.Repository
{
    public abstract class AbsSqlRepository<T>(Database database) : AbsSqlDtoRepository<T,T>(database)
        where T : class
    {
        public virtual string GetKey(T item) => base.GetTKey(item);
        public override string GetTKey(T item) => GetKey(item);
        public override string GetDKey(T item) => GetKey(item);
        protected override T Map(T item)=> item;
    }

    public abstract class AbsSqlIntRepository<T>(Database database) : AbsSqlDtoIntRepository<T, T>(database)
        where T : class
    {
        public virtual  int GetKey(T item) => base.GetTKey(item);
        public override int GetTKey(T item) => GetKey(item);
        public override int GetDKey(T item) => GetKey(item);
        protected override T Map(T item)=> item;
    }

}