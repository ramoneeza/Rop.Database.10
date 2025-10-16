using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Rop.Dapper.ContribEx10;
using xUnit.Rop.Dapper.ContribEx10.Data;

namespace xUnit.Rop.Dapper.ContribEx10
{
    public class PartialKeyTest
    {
        public PartialKeyTest()
        {
        }

        private (CarAutoKey,CarAutoKey,CarAutoKey) _insertCar(IDbConnection conn)
        {
            var item1 = new CarAutoKey() { Model = "Ford" };
            var item2 = new CarAutoKey() { Model = "Chevrolet" };
            var item3 = new CarAutoKey() { Model = "Toyota" };
            conn.Insert(item1);
            conn.Insert(item2);
            conn.Insert(item3);
            return (item1,item2,item3);
        }
        private void _insertPartials(IDbConnection conn,int baseid1,int baseid2)
        {
            var item1=new CarOwnerPartialkeyInt(){IdCar = baseid1, Account = "1",Name =  "Pepe"};
            var item2=new CarOwnerPartialkeyInt(){IdCar = baseid1, Account = "2", Name = "Antonio"};
            var item3=new CarOwnerPartialkeyInt(){IdCar = baseid2, Account = "3", Name = "Juan"};
            conn.Insert(item1);
            conn.Insert(item2);
            conn.Insert(item3);
        }
        [Fact]
        public async Task DeletePartialTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var (a1,a2,a3)=_insertCar(conn);
            _insertPartials(conn,a1.Id,a2.Id);
            var r1 = conn.DeleteByPartialKey<CarOwnerPartialkeyInt>(a1.Id);
            Assert.Equal(2,r1);
            var r2 = conn.DeleteByPartialKey<CarOwnerPartialkeyInt>(a2.Id);
            Assert.Equal(1,r2);
            // Already deleted
            r1 = conn.DeleteByPartialKey<CarOwnerPartialkeyInt>(a1.Id);
            Assert.Equal(0,r1);
            r2 = conn.DeleteByPartialKey<CarOwnerPartialkeyInt>(a2.Id);
            Assert.Equal(0,r2);
        }

        [Fact]
        public async Task GetPartialTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var (a1,a2,a3)=_insertCar(conn);
            _insertPartials(conn,a1.Id,a2.Id);
            var result = conn.GetPartial<CarOwnerPartialkeyInt>(a1.Id).OrderBy(m=>m.IdCar).ThenBy(m=>m.Account).ToList();
            Assert.Equal(2, result.Count());
            Assert.Equal(a1.Id, result[0].IdCar);
            Assert.Equal("1",result[0].Account);
            Assert.Equal("Pepe",result[0].Name);
            Assert.Equal(1, result[1].IdCar);
            Assert.Equal("2", result[1].Account);
            Assert.Equal("Antonio",result[1].Name);
        }

        [Fact]
        public async Task GetPartial2_Test()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var car = new CarAutoKey() { Model = "P2C" };
            conn.Insert(car);
            var p1 = new CarOwnerPartialkeyInt() { IdCar = car.Id, Account = "A1", Name = "N1" };
            var p2 = new CarOwnerPartialkeyInt() { IdCar = car.Id, Account = "A2", Name = "N2" };
            conn.Insert(p1); conn.Insert(p2);
            var byAccount = conn.GetPartial2<CarOwnerPartialkeyInt>("A2").ToList();
            Assert.Single(byAccount);
            Assert.Equal("N2", byAccount[0].Name);
        }

        [Fact]
        public async Task InsertOrUpdatePartialTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var a = new CarAutoKey() { Model = "G1" };
            conn.Insert(a);

            var items = new List<CarOwnerPartialkeyInt>(){
                new CarOwnerPartialkeyInt(){ Account = "X", Name = "Nx" },
                new CarOwnerPartialkeyInt(){ Account = "Y", Name = "Ny" }
            };
            // use transaction
            using var tr = conn.BeginTransaction();
            var ok = conn.InsertOrUpdatePartial<CarOwnerPartialkeyInt>(a.Id, items, tr);
            tr.Commit();
            Assert.True(ok);
            var part = conn.GetPartial<CarOwnerPartialkeyInt>(a.Id).ToList();
            Assert.Equal(2, part.Count);
        }

    }

}
