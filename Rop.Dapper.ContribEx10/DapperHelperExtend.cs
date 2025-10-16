using Dapper.Contrib.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Extends Dapper to obtain internal data
/// </summary>
public static partial class DapperHelperExtend
{
    /// <summary>
    /// Access to ExplicitKey Properties Cache
    /// </summary>
    /// <param name="type"></param>
    /// <returns>List of properties</returns>
    public static List<PropertyInfo> ExplicitKeyPropertiesCache(Type type)=>ExplicitKeyPropertiesCacheInfo.FastInvoke<List<PropertyInfo>>(type);
    /// <summary>
    /// Access to Key Properties Cache
    /// </summary>
    /// <param name="type"></param>
    /// <returns>List of properties</returns>
    public static List<PropertyInfo> KeyPropertiesCache(Type type)=>KeyPropertiesCacheInfo.FastInvoke<List<PropertyInfo>>(type);
    /// <summary>
    /// Get table name for a class
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Table name</returns>
    public static string GetTableName(Type type) =>GetTableNameInfo.FastInvoke<string>(type);
    /// <summary>
    /// Access to Type Properties Cache
    /// </summary>
    /// <param name="type"></param>
    /// <returns>List of properties</returns>
    public static List<PropertyInfo> TypePropertiesCache(Type type)=>TypePropertiesCacheInfo.FastInvoke<List<PropertyInfo>>(type);
    /// <summary>
    /// Access to Computed Properties Cache
    /// </summary>
    /// <param name="type"></param>
    /// <returns>List of properties</returns>
    public static List<PropertyInfo> ComputedPropertiesCache(Type type) =>ComputedPropertiesCacheInfo.FastInvoke<List<PropertyInfo>>(type);
    /// <summary>
    /// Access to connection formatter
    /// </summary>
    /// <param name="connection"></param>
    /// <returns>ISqlAdaptar</returns>
    public static ISqlAdapter GetFormatter(IDbConnection connection)=>GetFormatterInfo.FastInvoke<ISqlAdapter>(connection);

    private static ConcurrentDictionary<RuntimeTypeHandle, string> _getQueries=>GetQueriesField.Value;

    
    /// <summary>
    /// Convert propertyinfo to names
    /// </summary>
    /// <param name="props"></param>
    /// <returns>List of columns names</returns>
    public static IEnumerable<string> GetColumnNames(IEnumerable<PropertyInfo> props)=>props.Select(p => p.Name);
    /// <summary>
    /// Retrieves the column names for a given type, with an option to exclude auto-generated key columns.
    /// </summary>
    /// <param name="type">The type for which to retrieve column names.</param>
    /// <param name="excludeautoKey">
    /// A boolean value indicating whether to exclude columns that are auto-generated keys.
    /// If <c>true</c>, auto-generated key columns will be excluded.
    /// </param>
    /// <returns>An enumerable collection of column names for the specified type.</returns>
    public static IEnumerable<string> GetColumnNames(Type type,bool excludeautoKey)
    {
        var cn= GetColumnNames(TypePropertiesCache(type));
        if (excludeautoKey)
        {
            var k = GetAnyKeyDescription(type);
            if (k is { IsAutoKey: true }) cn = cn.Where(c => c != k.KeyName);
        }
        return cn;
    }

    /// <summary>
    /// Access to Cache for Get query
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Sql string</returns>
    public static string SelectGetCache(Type type)
    {
        return _getQueries.GetOrFactory(type, _ =>
        {
            var (key,_) = GetSingleKey(type);
            var name = GetTableName(type);
            return $"select * from {name} where {key.Name} = @id";
        });
    }
    /// <summary>
    /// Access to Cache for GetAll query
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Sql String</returns>
    public static string SelectGetAllCache(Type type)
    {
        
        var lstgeneric = typeof(List<>);
        var listoft = lstgeneric.MakeGenericType(type);
        return _getQueries.GetOrFactory(listoft, _ =>
        {
            var name = GetTableName(type);
            return $"select * from {name}";
        });
    }
    /// <summary>
    /// Access to Cache for Delete query
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Sql String</returns>
    public static string DeleteByKeyCache(Type type)
    {
        return DeleteByKeyDic.GetOrFactory(type, _ =>
        {
            var (key, _) = GetSingleKey(type);
            var name = GetTableName(type);
            return $"DELETE FROM {name} WHERE {key.Name} = @id";
        });
    }
    /// <summary>
    /// Access to Cache for Delete Partial query
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Sql String</returns>
    public static string DeleteByPartialKeyCache(Type type)
    {
        return DeleteByPartialKeyDic.GetOrFactory(type, _ =>
        {
            var kd = GetPartialKeyDescription(type);
            var name = kd.TableName;
            var key = kd.KeyName;
            return $"DELETE FROM {name} WHERE {key} = @id";
        });
    }
    /// <summary>
    /// Gets the name of the foreign database for a given type, using the ForeignDatabaseAttribute or table name convention.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>Name of the foreign database, or empty string if not found.</returns>
    public static string GetForeignDatabaseName(Type type)
    {
        return ForeignDatabase.GetOrFactory(type, _ =>
        {
            var foreigndatabaseAttrName = type.GetCustomAttribute<ForeignDatabaseAttribute>(false)?.Name;
            if (foreigndatabaseAttrName!= null) return foreigndatabaseAttrName;
            var tname = GetTableName(type) ?? "";
            var sp = tname.Split('.');
            return sp.Length == 3 ? sp[0] : "";
        });
    }
    /// <summary>
    /// Gets a cached SQL SELECT statement for all columns of a type (slim select).
    /// </summary>
    /// <param name="type">The type for which to generate the SELECT SQL.</param>
    /// <returns>SQL SELECT statement as a string.</returns>
    public static string SelectGetAllSlimCache(Type type)
    {
        return SelectSlimDic.GetOrFactory(type, _ =>
        {
            var name = GetTableName(type);
            var allProperties = TypePropertiesCache(type);
            var proplst = string.Join(", ", allProperties.Select(p => p.Name));
            return $"select {proplst} from {name}";
        });
    }
    /// <summary>
    /// Gets a cached SQL SELECT statement for a single entity by key (slim select).
    /// </summary>
    /// <param name="type">The type for which to generate the SELECT SQL.</param>
    /// <returns>SQL SELECT statement as a string.</returns>
    public static string SelectGetSlimCache(Type type)
    {
        return GetSlimDic.GetOrFactory(type, _ =>
        {
            var select = SelectGetAllSlimCache(type);
            var (key, _) = GetSingleKey(type);
            return  $"{select} where {key.Name} = @id";
        });
    }
    /// <summary>
    /// Gets a cached SQL SELECT statement for entities by partial key.
    /// </summary>
    /// <param name="type">The type for which to generate the SELECT SQL.</param>
    /// <returns>SQL SELECT statement as a string.</returns>
    public static string SelectGetPartialCache(Type type)
    {
        return GetPartialDic.GetOrFactory(type, _ =>
        {
            var kd = GetPartialKeyDescription(type)??throw new ArgumentException($"{type} has not partial key");
            return $"SELECT * FROM {kd.TableName} where {kd.KeyName} = @id";
        });
    }
    /// <summary>
    /// Gets a cached SQL SELECT statement for entities by the second partial key.
    /// </summary>
    /// <param name="type">The type for which to generate the SELECT SQL.</param>
    /// <returns>SQL SELECT statement as a string.</returns>
    public static string SelectGetPartial2Cache(Type type)
    {
        return GetPartial2Dic.GetOrFactory(type, _ =>
        {
            var kd = GetPartialKeyDescription(type) ?? throw new ArgumentException($"{type} has not partial key");
            return $"SELECT * FROM {kd.TableName} where {kd.Key2Name} = @id";
        });
    }
    /// <summary>
    /// Gets a cached SQL SELECT statement for entities by two partial keys.
    /// </summary>
    /// <param name="type">The type for which to generate the SELECT SQL.</param>
    /// <returns>SQL SELECT statement as a string.</returns>
    public static string SelectGetPartial12Cache(Type type)
    {
        return GetPartial12Dic.GetOrFactory(type, _ =>
        {
            var kd = GetPartialKeyDescription(type) ?? throw new ArgumentException($"{type} has not partial key");
            if (string.IsNullOrEmpty(kd.KeyName)||string.IsNullOrEmpty(kd.Key2Name)) throw new ArgumentException($"{type} has not two partial keys");
            return $"SELECT * FROM {kd.TableName} where {kd.KeyName} = @id1 and {kd.Key2Name} = @id2";
        });
    }

    /// <summary>
    /// Get Expression's member name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression">Expression</param>
    /// <returns>Member name</returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string GetMemberName<T>(this Expression<T> expression)
    {
        switch (expression.Body)
        {
            case MemberExpression m:
                return m.Member.Name;
            case UnaryExpression u:
                if (u.Operand is MemberExpression m2) return m2.Member.Name;
                throw new NotImplementedException(expression.GetType().ToString());
            default:
                throw new NotImplementedException(expression.GetType().ToString());
        }
    }
    /// <summary>
    /// Convert Ienumerable of int keys to string
    /// </summary>
    /// <param name="ids">IEnumerable of keys</param>
    /// <returns>string</returns>
    public static string GetIdList(IEnumerable<int> ids)=>string.Join(",", ids);
        
    /// <summary>
    /// Convert Ienumerable of string keys to string
    /// </summary>
    /// <param name="ids">IEnumerable of keys</param>
    /// <returns>string</returns>
    public static string GetIdList(IEnumerable<string> ids)=>string.Join(",", ids.Select(i=>$"'{i}'"));

    /// <summary>
    /// Convert Ienumerable of dynamic keys to string
    /// </summary>
    /// <param name="ids">IEnumerable of keys</param>
    /// <returns>string</returns>
    public static string GetIdListDyn(IEnumerable ids)
    {
        var idsobj = ids.Cast<object>().ToArray();
        if (idsobj.Length == 0) return "";
        var id0 = idsobj[0];
        return id0 is string ? GetIdList(idsobj.Cast<string>()) : GetIdList(idsobj.Cast<int>());
    }

    /// <summary>
    /// Gets a cached SQL INSERT statement for a type, ignoring key attributes.
    /// </summary>
    /// <param name="type">The type for which to generate the INSERT SQL.</param>
    /// <returns>SQL INSERT statement as a string.</returns>
    public static string InsertNoKeyAttCache(Type type)
    {
        return InsertNoKeyAttDic.GetOrFactory(type, _ =>
        {
            var tableName = GetTableName(type);
            List<PropertyInfo> first = TypePropertiesCache(type);
            List<PropertyInfo> second = ComputedPropertiesCache(type);
            List<PropertyInfo> list = first.Except(second).ToList();
            var columns = string.Join(", ", list.Select(p => p.Name));
            var values = string.Join(", ", list.Select(p => $"@{p.Name}"));
            var sql = $"insert into {tableName} ({columns}) values ({values})";
            return sql;
        });
    }

    /// <summary>
    /// Gets a cached SQL MERGE statement for insert or update operations for types without auto key.
    /// </summary>
    /// <param name="type">The type for which to generate the MERGE SQL.</param>
    /// <returns>SQL MERGE statement as a string.</returns>
    public static string InsertOrUpdateMergeCache(Type type)
    {
        return InsertOrUpdateMergeDic.GetOrFactory(type, _ =>
        {
            var kd = GetKeyDescription(type);
            if (kd.IsAutoKey) throw new ArgumentException($"{type} has auto key");
            var tableName = kd.TableName;
            var keyName = kd.KeyName;
            var properties = TypePropertiesCache(type).Where(c => c.Name != keyName).ToList();
            var propnames = GetColumnNames(properties).ToList();
            var columns = string.Join(", ", propnames);
            var values = string.Join(", ", propnames.Select(p => $"@{p}"));
            var updates = string.Join(", ", propnames.Select(p => $"{p} = @{p}"));
            var sql = $"""

                               MERGE INTO {tableName} AS target
                               USING (SELECT @{keyName} AS {keyName}, {values}) AS source
                               ON (target.{keyName} = source.{keyName})
                               WHEN MATCHED THEN 
                                   UPDATE SET {updates}
                               WHEN NOT MATCHED THEN
                                   INSERT ({keyName}, {columns})
                                   VALUES (@{keyName}, {values});
                           
                       """;
            return sql;
        });
    }
}