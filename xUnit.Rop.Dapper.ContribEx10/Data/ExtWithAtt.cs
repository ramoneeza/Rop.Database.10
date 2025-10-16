using Dapper.Contrib.Extensions;
using Rop.Dapper.ContribEx10;

namespace xUnit.Rop.Dapper.ContribEx10.Data
{
    [Table("Intranet.dbo.Permisos")]
    public class ExtWithAttTable
    {
        public string Aplicacion { get; init; } = "";
        public string Cuenta { get; init; } = "";
        public string Role { get; init; } = "";
    }
    [ForeignDatabase("Intranet")]
    [Table("Permisos")]
    public class ExtWithAttTable2
    {
        public string Aplicacion { get; init; } = "";
        public string Cuenta { get; init; } = "";
        public string Role { get; init; } = "";
    }
    [ForeignDatabase("Intranet")]
    [Table("Permisos")]
    public class ExtWithKey1
    {
        [ExplicitKey]
        public string Cuenta { get; init; } = "";
        public string Role { get; init; } = "";
    }
    [Table("Intranet.dbo.Permisos")]
    public class ExtWithKey2
    {
        [ExplicitKey]
        public string Cuenta { get; init; } = "";
        public string Role { get; init; } = "";
    }
}
