using System.Collections.Concurrent;
using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Helper methods for working with dictionaries and attributes using reflection.
/// </summary>
public static class Helper
{
    /// <summary>
    /// Gets the value associated with a type in the dictionary, or creates it using the provided function if it does not exist.
    /// </summary>
    /// <typeparam name="K">Type of value stored in the dictionary.</typeparam>
    /// <param name="dic">Concurrent dictionary with <see cref="RuntimeTypeHandle"/> as key.</param>
    /// <param name="type">Type to look up in the dictionary.</param>
    /// <param name="fn">Function to create the value if it does not exist.</param>
    /// <returns>Value associated with the type.</returns>
    public static K GetOrFactory<K>(this ConcurrentDictionary<RuntimeTypeHandle, K> dic,Type type, Func<Type, K> fn)
    {
        if (!dic.TryGetValue(type.TypeHandle, out var sql))
        {
            sql= fn(type);
            dic[type.TypeHandle] = sql;
        }
        return sql;
    }

    /// <summary>
    /// Gets the properties that have a specific attribute, along with the attribute instance.
    /// </summary>
    /// <typeparam name="A">Type of attribute to search for.</typeparam>
    /// <param name="props">Collection of properties.</param>
    /// <param name="inherited">Indicates whether to search for inherited attributes.</param>
    /// <returns>Enumeration of tuples (PropertyInfo, attribute).</returns>
    public static IEnumerable<(PropertyInfo property, A attr)> GetPropertyWithAttribute<A>(this IEnumerable<PropertyInfo> props,bool inherited)
    {
        foreach (var propertyInfo in props)
        {
            var lst = propertyInfo.GetCustomAttributes(inherited).OfType<A>().ToArray();
            if (lst.Length>0) yield return (propertyInfo, lst[0]);
        }    
    }
}