using System;
using Dapper.Contrib.Extensions;

namespace xUnit.Rop.Dapper.ContribEx10.Data
{
    [Table("Car_AutoKey")]
    public record CarAutoKeyJoin
    {
        [Key]
        public int Id { get; init; }

        public string Model { get; init; } = "";
        public string SubModel { get; init; } = "";

        [Computed]
        public CarOwnerPartialkeyInt[] Maniobras { get; set; } = [];
    }

    [Table("Car_ExplicitKey")]
    public record CarExplicitKeyJoin
    {
        [ExplicitKey]
        public string Id { get; init; } = "";

        public string Model { get; init; } = "";
        public string SubModel { get; init; } = "";

        [Computed]
        public CarOwnerPartialkeyStr[] Maniobras { get; set; } = [];
    }
}