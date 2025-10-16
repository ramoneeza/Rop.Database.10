using Rop.Database10;
namespace Rop.Database10
{
    public partial class Database
    {
        public Result<long> GetDbVersion()
        {
            return UnitOfWorkSingle(conn=>conn.GetDbVersion());
        }
        public Result<long> GetDbVersion(KeyDescription table)
        {
            return UnitOfWorkSingle(conn=>conn.GetDbVersion(table));
        }
        public Result<long> GetDbVersion(string use)
        {
            return UnitOfWorkSingle(conn=>conn.GetDbVersion(use));
        }

        public async Task<Result<long>> GetDbVersionAsync()
        {
            return await UnitOfWorkSingleAsync(conn => conn.GetDbVersionAsync());
        }
        public async Task<Result<long>> GetDbVersionAsync(KeyDescription table)
        {
            return await UnitOfWorkSingleAsync(conn => conn.GetDbVersionAsync(table));
        }
        public async Task<Result<long>> GetDbVersionAsync(string use)
        {
            return await UnitOfWorkSingleAsync(conn => conn.GetDbVersionAsync(use));
        }
        
        public VoidResult VerifyNotify(KeyDescription td)
        {
            var r = GetDbVersion(td);
            if (r.IsFailed) return r;
            var r2=GetTableChanges(0,td);
            return r2;
        }

        public VoidResult VerifyNotify<T>() where T : class
        {
            var td=GetAnyKeyDescription<T>();
            if (td == null) return new Error($"Table {typeof(T).Name} not found");
            return VerifyNotify(td);
        }

        public async Task<VoidResult> VerifyNotifyAsync(KeyDescription td)
        {
            var r = await GetDbVersionAsync(td);
            if (r.IsFailed) return r;
            var r2 = await GetTableChangesAsync(0, td);
            return r2;
        }

        public async Task<VoidResult> VerifyNotifyAsync<T>() where T : class
        {
            var td = GetAnyKeyDescription<T>();
            if (td == null) return new Error($"Table {typeof(T).Name} not found");
            return await VerifyNotifyAsync(td);
        }

        public void VerifyThrowNotify(KeyDescription td)
        {
            VerifyNotify(td).ThrowIfFailed();
        }
        public void VerifyThrowNotify<T>()
        {
            var td=GetAnyKeyDescription<T>()??throw new Exception($"Table for {typeof(T).Name} not found");
            VerifyThrowNotify(td);
        }

        public async Task VerifyThrowNotifyAsync(KeyDescription td)
        {
            var r = await VerifyNotifyAsync(td);
            r.ThrowIfFailed();
        }

        public async Task VerifyThrowNotifyAsync<T>() where T : class
        {
            var td = GetAnyKeyDescription<T>() ?? throw new Exception($"Table for {typeof(T).Name} not found");
            await VerifyThrowNotifyAsync(td);
        }

        public Result<DeltaChanges> GetTableChanges(long lasttableVersion,KeyDescription td)
        {
            return UnitOfWorkSingle(conn => conn.GetTableChanges(lasttableVersion,td));
        }
        
        public async Task<Result<DeltaChanges>> GetTableChangesAsync(long lasttableVersion, KeyDescription td)
        {
            return await UnitOfWorkSingleAsync(conn => conn.GetTableChangesAsync(lasttableVersion, td));
        }


       
    }
}
