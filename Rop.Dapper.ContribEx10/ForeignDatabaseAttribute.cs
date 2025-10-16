namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Attribute to mark a class as belonging to a foreign database.
/// </summary>
[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
public class ForeignDatabaseAttribute : Attribute
{
    /// <summary>
    /// Name of the foreign database.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes the attribute with the name of the foreign database.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    public ForeignDatabaseAttribute(string databaseName)
    {
        Name = databaseName;
    }
}