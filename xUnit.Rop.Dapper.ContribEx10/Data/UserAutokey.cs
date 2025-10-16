using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Rop.Dapper.ContribEx10;

namespace xUnit.Rop.Dapper.ContribEx10.Data;

[Table("User_Autokey")]
public record UserAutokey
{
    [Key]
    public int Id { get; init; }

    public string Name { get; init; } = "";
    public string Surname { get; init; } = "";
    public DateTime? Created { get; init; }
}

[Table("Car_ExplicitKey")]
public record CarExplicitKey
{
    [ExplicitKey]
    public string Id { get; init; } = "";
    public string Model { get; init; } = "";
    public string SubModel { get; init; } = "";
}
[Table("Car_AutoKey")]
public record CarAutoKey
{
    [Key]
    public int Id { get; init; }
    public string Model { get; init; } = "";
    public string SubModel { get; init; } = "";
}


[Table("Car_Owner_PartialKeyStr")]
public record CarOwnerPartialkeyStr
{
    [PartialKey(0)]
    public string IdCar { get; init; } = "";

    [PartialKey(1)]
    public string Account { get; init; } = "";

    public string Name { get; init; } = "";
}
[Table("Car_Owner_PartialKeyInt")]
public record CarOwnerPartialkeyInt
{
    [PartialKey(0)]
    public int IdCar { get; init; }

    [PartialKey(1)]
    public string Account { get; init; } = "";

    public string Name { get; init; } = "";
}

