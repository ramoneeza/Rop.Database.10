using System.Reflection;

namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Interface for key description in entities.
/// </summary>
public interface IKeyDescription
{
    /// <summary>
    /// Name of the associated table.
    /// </summary>
    string TableName { get; }
    /// <summary>
    /// Name of the key column.
    /// </summary>
    string KeyName { get; }
    /// <summary>
    /// Key property information.
    /// </summary>
    PropertyInfo KeyProp { get; }
    /// <summary>
    /// Indicates whether the key type is string.
    /// </summary>
    bool KeyTypeIsString { get; }
    /// <summary>
    /// Indicates whether the table is from a foreign database.
    /// </summary>
    bool IsForeignTable { get; }
    /// <summary>
    /// Indicates whether the key is auto-generated.
    /// </summary>
    bool IsAutoKey { get; }
    /// <summary>
    /// Name of the associated foreign database.
    /// </summary>
    string ForeignDatabaseName { get; }
    /// <summary>
    /// Returns the USE command for the foreign database if applicable.
    /// </summary>
    /// <returns>USE command or empty string.</returns>
    string GetUse();
    /// <summary>
    /// Gets the key value for the given object.
    /// </summary>
    /// <param name="item">Entity instance.</param>
    /// <returns>Key value.</returns>
    object GetKeyValue(object item);
}

