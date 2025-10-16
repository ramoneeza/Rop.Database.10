using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Rop.Database10
{
    public static partial class ConnectionHelper
    {
        public static Result<long> GetDbVersion(this DbConnection conn)
        {
            try
            {
                var r = conn.ExecuteScalar("SELECT CHANGE_TRACKING_CURRENT_VERSION();");
                return (r is long l) ? l : -1;
            }
            catch (Exception ex)
            {
                return new ExceptionError(ex);
            }
        }
        public static async Task<Result<long>> GetDbVersionAsync(this DbConnection conn)
        {
            try
            {
                var r = await conn.ExecuteScalarAsync("SELECT CHANGE_TRACKING_CURRENT_VERSION();");
                return (r is long l) ? l : -1;
            }
            catch (Exception ex)
            {
                return new ExceptionError(ex);
            }
        }

        public static Result<long> GetDbVersion(this DbConnection conn,KeyDescription td)
        {
            var use = td.GetUse();
            return GetDbVersion(conn, use);
        }
        public static async Task<Result<long>> GetDbVersionAsync(this DbConnection conn, KeyDescription td)
        {
            var use = td.GetUse();
            return await GetDbVersionAsync(conn, use);
        }

        public static Result<long> GetDbVersion(this DbConnection conn,string use)
        {
            try
            {
                var r = conn.ExecuteScalar($"{use}SELECT CHANGE_TRACKING_CURRENT_VERSION();");
                return (r is long l) ? l : 0;
            }
            catch (Exception ex)
            {
                return new ExceptionError(ex);
            }
        }
        public static async Task<Result<long>> GetDbVersionAsync(this DbConnection conn, string use)
        {
            try
            {
                var r = await conn.ExecuteScalarAsync($"{use}SELECT CHANGE_TRACKING_CURRENT_VERSION();");
                return (r is long l) ? l : 0;
            }
            catch (Exception ex)
            {
                return new ExceptionError(ex);
            }
        }
        public static Result<DeltaChanges> GetTableChanges(this DbConnection conn,long lastv,KeyDescription td)
        {
            var use = td.GetUse();
            // ReSharper disable once StringLiteralTypo
            var cmdStr = (!(td is PartialKeyDescription ptd))
                ? $"{use}SELECT {td.KeyName} as Id,SYS_CHANGE_VERSION,SYS_CHANGE_CREATION_VERSION,SYS_CHANGE_OPERATION FROM changetable(changes {td.TableName},@lastversion) as CT ORDER BY SYS_CHANGE_VERSION"
                : $"{use}SELECT {ptd.KeyName} as Id, {ptd.Key2Name} as Id2 ,SYS_CHANGE_VERSION,SYS_CHANGE_CREATION_VERSION,SYS_CHANGE_OPERATION FROM changetable(changes {ptd.TableName},@lastversion) as CT ORDER BY SYS_CHANGE_VERSION";
            var r = conn.Query<ChangeRowDto>(cmdStr,new {lastversion=lastv}).ToList();
            if (r.Count == 0) return DeltaChanges.Empty(td,lastv);
            var cv = r.Max(cr => cr.SYS_CHANGE_VERSION);
            var all = r.Select(dto => dto.ToChangeRow()).Where(c => c.Operation != ChangeOperation.Unknown);
            return new DeltaChanges(td,cv, lastv, all);
        }
        public static async Task<Result<DeltaChanges>> GetTableChangesAsync(this DbConnection conn, long lastv, KeyDescription td)
        {
            var use = td.GetUse();
            // ReSharper disable once StringLiteralTypo
            var cmdStr = (!(td is PartialKeyDescription ptd))
                ? $"{use}SELECT {td.KeyName} as Id,SYS_CHANGE_VERSION,SYS_CHANGE_CREATION_VERSION,SYS_CHANGE_OPERATION FROM changetable(changes {td.TableName},@lastversion) as CT ORDER BY SYS_CHANGE_VERSION"
                : $"{use}SELECT {ptd.KeyName} as Id, {ptd.Key2Name} as Id2 ,SYS_CHANGE_VERSION,SYS_CHANGE_CREATION_VERSION,SYS_CHANGE_OPERATION FROM changetable(changes {ptd.TableName},@lastversion) as CT ORDER BY SYS_CHANGE_VERSION";
            var r = (await conn.QueryAsync<ChangeRowDto>(cmdStr, new { lastversion = lastv })).ToList();
            if (r.Count == 0) return DeltaChanges.Empty(td,lastv);
            var cv = r.Max(cr => cr.SYS_CHANGE_VERSION);
            var all = r.Select(dto => dto.ToChangeRow()).Where(c => c.Operation != ChangeOperation.Unknown);
            return new DeltaChanges(td,cv, lastv, all);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class ChangeRowDto
        {
            public dynamic Id { get; init; } = null!;
            public dynamic? Id2 { get; init; } = null;
            public long SYS_CHANGE_VERSION { get; init; }
            public long SYS_CHANGE_CREATION_VERSION { get; init; }
            public string SYS_CHANGE_OPERATION { get; init; } = "";
            public ChangeRow ToChangeRow()
            {
                var op = ChangeOperation.Unknown;
                switch (SYS_CHANGE_OPERATION)
                {
                    case "D": op = ChangeOperation.Delete;
                        break;
                    case "I": op = ChangeOperation.Insert;
                        break;
                    case "U": op = ChangeOperation.Update;
                        break;
                }
                return new ChangeRow(Id, Id2, op);
            }
        }
    }
}
