using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace Rop.Dapper.ContribEx10;

public static partial class DapperHelperExtend
{
    private static readonly MethodInfo ExplicitKeyPropertiesCacheInfo = _getInfo("ExplicitKeyPropertiesCache");

    private static readonly MethodInfo KeyPropertiesCacheInfo= _getInfo("KeyPropertiesCache");

    private static readonly MethodInfo GetTableNameInfo= _getInfo("GetTableName");

    private static readonly MethodInfo TypePropertiesCacheInfo= _getInfo("TypePropertiesCache");
    
    private static readonly MethodInfo ComputedPropertiesCacheInfo= _getInfo("ComputedPropertiesCache");

    private static readonly MethodInfo GetFormatterInfo= _getInfo("GetFormatter");

    private static readonly ReadOnlyFieldCache<ConcurrentDictionary<RuntimeTypeHandle, string>> GetQueriesField = new(typeof(SqlMapperExtensions), "GetQueries");

    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> ForeignDatabase = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> SelectSlimDic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetSlimDic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetPartialDic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetPartial2Dic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> GetPartial12Dic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> DeleteByKeyDic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> DeleteByPartialKeyDic = new();
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertNoKeyAttDic = new();

    private static readonly ConcurrentDictionary<RuntimeTypeHandle, string> InsertOrUpdateMergeDic = new();
    
    static DapperHelperExtend()
    {
    }
    private static MethodInfo _getInfo(string name) => typeof(SqlMapperExtensions).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"Invalid method {name} in static constructor");
} 
        

