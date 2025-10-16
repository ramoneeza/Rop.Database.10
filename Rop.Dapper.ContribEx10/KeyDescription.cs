using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Immutable class with the main key description for an entity.
/// </summary>
public class KeyDescription : IKeyDescription
{
    /// <summary>
    /// Name of the associated table.
    /// </summary>
    public string TableName { get; }
    /// <summary>
    /// Name of the key column.
    /// </summary>
    public string KeyName { get; }
    public PropertyCache KeyPropCache { get; }
    /// <summary>
    /// Key property information.
    /// </summary>
    public PropertyInfo KeyProp=>KeyPropCache.Property;
    /// <summary>
    /// Type of the key.
    /// </summary>
    public Type KeyType=> KeyProp.PropertyType;
    /// <summary>
    /// Indicates whether the key type is string.
    /// </summary>
    public bool KeyTypeIsString { get; }
    /// <summary>
    /// Indicates whether the table belongs to a foreign database.
    /// </summary>
    public bool IsForeignTable { get; }
    /// <summary>
    /// Indicates whether the key is auto-generated.
    /// </summary>
    public bool IsAutoKey { get; }
    /// <summary>
    /// Indicates whether the key is partial.
    /// </summary>
    public virtual bool IsPartialKey => false;
    /// <summary>
    /// Name of the associated foreign database.
    /// </summary>
    public string ForeignDatabaseName { get; }
    /// <summary>
    /// Returns the USE command for the foreign database if applicable.
    /// </summary>
    /// <returns>USE command or empty string.</returns>
    public string GetUse()
    {
        return (IsForeignTable) ? $"USE {ForeignDatabaseName}; " : "";
    }
    /// <summary>
    /// Gets the key value for the given object.
    /// </summary>
    /// <param name="item">Entity instance.</param>
    /// <returns>Key value.</returns>
    public object GetKeyValue(object item)
    {
        return KeyPropCache.Getter(item)??throw new Exception($"Key for {TableName} is null");
    }
    /// <summary>
    /// Initializes the main key description.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="keyName">Name of the key column.</param>
    /// <param name="keyProp">PropertyInfo of the key.</param>
    /// <param name="isAutoKey">Indicates whether the key is auto-generated.</param>
    /// <param name="foreignDatabaseName">Name of the foreign database.</param>
    public KeyDescription(string tableName, string keyName, PropertyInfo keyProp, bool isAutoKey,string foreignDatabaseName)
    {
        TableName = tableName;
        KeyName = keyName;
        KeyPropCache=new PropertyCache(keyProp);
        KeyTypeIsString = Type.GetTypeCode(KeyProp.PropertyType) == TypeCode.String;
        IsAutoKey = isAutoKey;
        ForeignDatabaseName = foreignDatabaseName;
        IsForeignTable= ForeignDatabaseName != "";
        IsAutoKey = isAutoKey;
    }
}