using System.Collections.Concurrent;
using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Provides helper methods for working with partial key descriptions in entities.
/// </summary>
public static partial class DapperHelperExtend
{
    /// <summary>
    /// Cache for partial key descriptions by type.
    /// </summary>
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, PartialKeyDescription> PartialKeyDescriptions = new();
    /// <summary>
    /// Gets the list of properties marked as partial keys for the specified type, ordered by their attribute order.
    /// </summary>
    /// <param name="type">Type to inspect.</param>
    /// <returns>List of PropertyInfo representing partial keys.</returns>
    private static List<PropertyInfo> _partialKeyProperties(Type type)
    {
        List<PropertyInfo> source2 = TypePropertiesCache(type);
        var list = source2.GetPropertyWithAttribute<PartialKeyAttribute>(true).OrderBy(t=>t.attr.Order).Select(t=>t.property).ToList();
        return list;
    }
    
    /// <summary>
    /// Gets the partial key description for the specified type.
    /// </summary>
    /// <param name="t">Type of the class.</param>
    /// <returns>PartialKeyDescription instance.</returns>
    public static PartialKeyDescription GetPartialKeyDescription(Type t)
    {
        return PartialKeyDescriptions.GetOrFactory(t, _ =>
        {
            var lst= _partialKeyProperties(t);
            if (lst.Count<2) throw new ArgumentException($"{t} has not two partial keys");
            var propkey = lst[0];
            var propkey2= lst[1];
            var keyname = propkey.Name;
            var key2Name=propkey2.Name;
            var tname = GetTableName(t);
            var fdb = GetForeignDatabaseName(t);
            return new PartialKeyDescription(tname, keyname, propkey,key2Name, propkey2,fdb);
        });
    }
    /// <summary>
    /// Gets the partial key description for the specified generic type.
    /// </summary>
    /// <typeparam name="T">Class type.</typeparam>
    /// <returns>PartialKeyDescription instance.</returns>
    public static PartialKeyDescription GetPartialKeyDescription<T>() where T:class => GetPartialKeyDescription(typeof(T));

    /// <summary>
    /// Gets the value of the first partial key for the given item.
    /// </summary>
    /// <typeparam name="T">Class type.</typeparam>
    /// <param name="item">Item instance.</param>
    /// <returns>Value of the first partial key.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static object? GetPartialKeyValue<T>(T item) where T:class
    {
        var kd = GetPartialKeyDescription<T>();
        return kd.GetKeyValue(item);
    }
    /// <summary>
    /// Gets the value of the second partial key for the given item.
    /// </summary>
    /// <typeparam name="T">Class type.</typeparam>
    /// <param name="item">Item instance.</param>
    /// <returns>Value of the second partial key.</returns>
    public static object? GetPartialKey2Value<T>(T item) where T : class
    {
        var kd = GetPartialKeyDescription<T>();
        return kd.GetKey2Value(item);
    }

    /// <summary>
    /// Gets the partial key description and the value of the first partial key for the given item.
    /// </summary>
    /// <typeparam name="T">Class type.</typeparam>
    /// <param name="item">Item instance.</param>
    /// <returns>Tuple with PartialKeyDescription and key value.</returns>
    public static (PartialKeyDescription keydescription, object key) GetPartialKeyDescriptionAndValue<T>(T item) where T:class
    {
        var kd = GetPartialKeyDescription<T>();
        var v = kd.GetKeyValue(item);
        return (kd, v);
    }
}