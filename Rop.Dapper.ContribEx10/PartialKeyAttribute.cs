namespace Rop.Dapper.ContribEx10;

/// <summary>
/// Attribute to mark a property as a partial key in an entity.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class PartialKeyAttribute : Attribute
{
    /// <summary>
    /// Order of the partial key in the entity.
    /// </summary>
    public int Order { get; set; }
    /// <summary>
    /// Initializes the attribute with the specified order.
    /// </summary>
    /// <param name="order">Order of the partial key.</param>
    public PartialKeyAttribute(int order)
    {
        Order = order;
    }
}
