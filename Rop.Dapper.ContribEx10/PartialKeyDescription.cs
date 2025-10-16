using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Immutable class with partial key description for an entity.
/// </summary>
public class PartialKeyDescription:KeyDescription
{
    /// <summary>
    /// Name of the second key column.
    /// </summary>
    public string Key2Name { get; }
    private readonly PropertyCache _key2PropertyCache;
    /// <summary>
    /// Information of the second key property.
    /// </summary>
    public PropertyInfo Key2Prop => _key2PropertyCache.Property;
    /// <summary>
    /// Indicates whether the second key type is string.
    /// </summary>
    public bool Key2TypeIsString { get; }
    /// <summary>
    /// Indicates that the key is partial.
    /// </summary>
    public override bool IsPartialKey => true;

    /// <summary>
    /// Initializes the partial key description.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="keyName">Name of the first key column.</param>
    /// <param name="keyProp">PropertyInfo of the first key.</param>
    /// <param name="key2Name">Name of the second key column.</param>
    /// <param name="key2Prop">PropertyInfo of the second key.</param>
    /// <param name="foreignDatabase">Name of the foreign database.</param>
    public PartialKeyDescription(string tableName, string keyName, PropertyInfo keyProp,string key2Name,PropertyInfo key2Prop,string foreignDatabase): base(tableName, keyName, keyProp,false,foreignDatabase)
    {
        Key2Name = key2Name;
        _key2PropertyCache=new PropertyCache(key2Prop);
        Key2TypeIsString = Type.GetTypeCode(Key2Prop.PropertyType) == TypeCode.String;
    }
    /// <summary>
    /// Gets the value of the second key for the given object.
    /// </summary>
    /// <param name="item">Entity instance.</param>
    /// <returns>Value of the second key.</returns>
    public object GetKey2Value(object item)
    {
        return _key2PropertyCache.Getter(item)??throw new Exception($"Key for {TableName} is null");
    }
    /// <summary>
    /// Gets the representation of both keys as a string separated by '|'
    /// </summary>
    /// <param name="item">Entity instance.</param>
    /// <returns>String with both keys.</returns>
    public string GetAllKeys(object item)
    {
        var key1 = GetKeyValue(item).ToString();
        var key2= GetKey2Value(item).ToString();
        return key1 + "|" + key2;
    }
    /// <summary>
    /// Deconstructs a composite key string into its individual values.
    /// </summary>
    /// <param name="key">Composite key string.</param>
    /// <returns>Tuple with both key values.</returns>
    public (object,object) DeconstructKey(string key)
    {
        var keys = key.Split('|');
        object key1 = (KeyTypeIsString)?keys[0]:int.Parse(keys[0]);
        object key2 = (Key2TypeIsString) ? keys[1] : int.Parse(keys[1]);
        return (keys[0], keys[1]);
    }
}

