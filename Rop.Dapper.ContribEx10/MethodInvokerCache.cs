using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rop.Dapper.ContribEx10;
/// <summary>
/// Provides helper methods for fast invocation of reflected methods and for caching property getters/setters.
/// </summary>
internal static class MethodInvokerCache
{
    /// <summary>
    /// Cache of delegates for fast invocation of reflected methods with a single parameter.
    /// </summary>
    private static readonly Dictionary<MethodInfo,Delegate> _invokerCache=new();
    /// <summary>
    /// Invokes a reflected method with a single parameter and returns the typed result.
    /// </summary>
    /// <typeparam name="TResult">Expected return type.</typeparam>
    /// <param name="method">MethodInfo to invoke.</param>
    /// <param name="arg1">Argument for the method.</param>
    /// <returns>Result of the invocation typed as <typeparamref name="TResult"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the return type is not compatible.</exception>
    public static TResult FastInvoke<TResult>(this MethodInfo method,object arg1)
    {
        if (!_invokerCache.TryGetValue(method, out var del))
        {
            var paramtype = method.GetParameters()[0].ParameterType;
            var delegateType = typeof(Func<,>).MakeGenericType(paramtype, method.ReturnType);
            del = method.CreateDelegate(delegateType);
            _invokerCache[method] = del;
        }
        var result=del.DynamicInvoke(arg1);
        if (result is not TResult r) throw new InvalidOperationException($"Invalid cast {result?.GetType()} to {typeof(TResult)}");
        return r;
    }
}
/// <summary>
/// Allows safe and typed retrieval of the value of a static readonly field.
/// </summary>
/// <typeparam name="T">Expected type of the field.</typeparam>
internal class ReadOnlyFieldCache<T>
{
    /// <summary>
    /// Cached value of the field.
    /// </summary>
    public readonly T Value;
    /// <summary>
    /// Initializes the cache by looking up the field by name and type in the specified class.
    /// </summary>
    /// <param name="type">Type containing the field.</param>
    /// <param name="fieldname">Name of the field.</param>
    /// <exception cref="ArgumentNullException">If the field does not exist.</exception>
    /// <exception cref="ArgumentException">If the field is not static, readonly, or of a compatible type.</exception>
    /// <exception cref="InvalidOperationException">If the value is not of the expected type.</exception>
    public ReadOnlyFieldCache(Type type,string fieldname)
    {
        var field=type.GetField(fieldname, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (field == null) throw new ArgumentNullException(nameof(field));
        if (!field.IsStatic) throw new ArgumentException("Field must be static", nameof(field));
        if (!field.IsInitOnly) throw new ArgumentException("Field must be readonly", nameof(field));
        if (!typeof(T).IsAssignableFrom(field.FieldType)) throw new ArgumentException($"Field type {field.FieldType} is not assignable to {typeof(T)}", nameof(field));
        Value=field.GetValue(null) is not T t ? throw new InvalidOperationException($"Invalid cast {field.FieldType} to {typeof(T)}") : t;
    }
}
/// <summary>
/// Provides fast and cached access to property getters and setters using compiled lambda expressions.
/// </summary>
public class PropertyCache
{
    /// <summary>
    /// Property information associated with this cache.
    /// </summary>
    public PropertyInfo Property { get; }
    /// <summary>
    /// Cached getter delegate for the property.
    /// </summary>
    public Func<object,object?> Getter { get; }
    /// <summary>
    /// Cached setter delegate for the property. Null if the property is read-only.
    /// </summary>
    public Action<object,object>? Setter { get; }

    private static Func<object, object?> _createGetter(PropertyInfo property)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var castInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var propertyAccess = Expression.Property(castInstance, property);
        var castResult = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<object, object?>>(castResult, instanceParam);
        return lambda.Compile();
    }
    private static Action<object, object?>? _createSetter(PropertyInfo property)
    {
        // Si la propiedad no tiene setter, retornar null
        if (!property.CanWrite || property.GetSetMethod() == null)
        {
            return null;
        }
        
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var valueParam = Expression.Parameter(typeof(object), "value");
        var castInstance = Expression.Convert(instanceParam, property.DeclaringType!);
        var castValue = Expression.Convert(valueParam, property.PropertyType);
        var propertySet = Expression.Assign(Expression.Property(castInstance, property), castValue);
        var lambda = Expression.Lambda<Action<object, object?>>(propertySet, instanceParam, valueParam);
        return lambda.Compile();
    }
    /// <summary>
    /// Initializes the property cache for the specified property.
    /// </summary>
    /// <param name="info">PropertyInfo to cache.</param>
    public PropertyCache(PropertyInfo info)
    {
        Property = info;
        Getter = _createGetter(info);
        Setter = _createSetter(info);
    }
}
