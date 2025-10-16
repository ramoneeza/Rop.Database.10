using System.Collections.Concurrent;
using System.Reflection;

namespace Rop.Dapper.ContribEx10;

public static partial class DapperHelperExtend
{
    private static readonly ConcurrentDictionary<RuntimeTypeHandle, KeyDescription> KeyDescriptions = new ConcurrentDictionary<RuntimeTypeHandle, KeyDescription>();

    /// <summary>
    /// Get Single Key for class of type t
    /// </summary>
    /// <param name="t">Type of class</param>
    /// <returns>Property and autokey flag</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static (PropertyInfo propkey, bool isautokey) GetSingleKey(Type t)
    {
        if (t == null) throw new ArgumentNullException(nameof(t));
        var keys = KeyPropertiesCache(t);
        var explicitKeys = ExplicitKeyPropertiesCache(t);
        var keyCount = keys.Count + explicitKeys.Count;
        if (keyCount != 1) throw new InvalidOperationException($"Type {t} has not single key");
        return (keys.Count > 0) ? (keys[0], true) : (explicitKeys[0], false);
    }
    
    public static bool TryGetKey(Type t,out PropertyInfo? propkey,out PropertyInfo? propkey2,out bool isautokey,out bool ispartial)
    {
        if (t == null) throw new ArgumentNullException(nameof(t));
        var keys = KeyPropertiesCache(t);
        var explicitKeys = ExplicitKeyPropertiesCache(t);
        var keyCount = keys.Count + explicitKeys.Count;
        if (keyCount == 1)
        {
            ispartial = false;
            propkey2 = null;
            if (keys.Count > 0)
            {
                propkey = keys[0];
                isautokey = true;
            }
            else
            {
                propkey = explicitKeys[0];
                isautokey = false;
            }
            return true;
        }
        var partialKeys = _partialKeyProperties(t);
        if (partialKeys.Count<2 || keyCount>1)
        {
            propkey = null;
            propkey2 = null;
            ispartial = false;
            isautokey = false;
            return false;
        }
        propkey = partialKeys[0];
        propkey2 = partialKeys[1];
        isautokey = false;
        ispartial = true;
        return true;
    }
    public static KeyDescription? GetAnyKeyDescription(Type t)
    {
        if (TryGetKey(t, out var _, out var _, out var isautokey, out var ispartial))
        {
            return ispartial? GetPartialKeyDescription(t) : GetKeyDescription(t);
        }
        return null;
    }

    /// <summary>
    /// Get Key Description for class of type t
    /// </summary>
    /// <param name="t">Type of class</param>
    /// <returns>KeyDescription</returns>
    public static KeyDescription GetKeyDescription(Type t)
    {
        return KeyDescriptions.GetOrFactory(t, _ =>
        {
            var (propkey, isautokey) = GetSingleKey(t);
            var keyname = propkey.Name;
            var tname = GetTableName(t);
            var fdb = GetForeignDatabaseName(t);
            return new KeyDescription(tname, keyname,propkey, isautokey,fdb);
        });
    }
    public static KeyDescription GetKeyDescription<T>() where T:class => GetKeyDescription(typeof(T));

    /// <summary>
    /// Get Key Value for item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">Item</param>
    /// <returns>Item's Key</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static object GetKeyValue<T>(T item)
    {
        var kd = GetKeyDescription(typeof(T));
        return kd.KeyProp.GetValue(item)??throw new InvalidOperationException("Key is null value");
    }
    /// <summary>
    /// Set Key for Item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SetKeyValue<T>(T item, object value)
    {
        var kd = GetSingleKey(typeof(T));
        kd.propkey.SetValue(item, value);
    }
    /// <summary>
    /// Get Key Description and Key Value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>Key description and Key value</returns>
    public static (KeyDescription keydescription, object value) GetKeyDescriptionAndValue<T>(T item) where T: class
    {
        var kd = GetKeyDescription<T>();
        var v =kd.GetKeyValue(item)??throw new InvalidOperationException("Key is null value");
        return (kd, v);
    }

}