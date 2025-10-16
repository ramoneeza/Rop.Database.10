using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Rop.Dapper.ContribEx10;
using xUnit.Rop.Dapper.ContribEx10.Data;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace xUnit.Rop.Dapper.ContribEx10
{
    public partial class ConnectionHelperTest
    {
        public ConnectionHelperTest()
        {
        }
        
        [Fact]
        public async Task OpenConnection()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            Assert.NotNull(conn);
        }

        [Fact]
        public async Task DeleteByKeyAutoKeyTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var r1 = conn.Insert(new UserAutokey(){ Name = "Hola" });
            // asertar que es mayor que 0
            Assert.True((int)r1 > 0);
            var r2 = conn.DeleteByKey<UserAutokey>(r1);
            Assert.True(r2);
            // Already deleted
            r2 = conn.DeleteByKey<UserAutokey>(r1);
            Assert.False(r2);
        }

        [Fact]
        public async Task DeleteByKeyExplicitKeyTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var r1 = conn.Insert(new CarExplicitKey() { Id = "MyKey", Model = "Hola" });
            var check=conn.Get<CarExplicitKey>("MyKey");
            Assert.NotNull(check);
            Assert.Equal("MyKey",check.Id);
            var r2 = conn.DeleteByKey<CarExplicitKey>(check.Id);
            Assert.True(r2);
            // Already deleted
            r2 = conn.DeleteByKey<CarExplicitKey>(r1);
            Assert.False(r2);
            check=conn.Get<CarExplicitKey>("MyKey");
            Assert.Null(check);
        }

        [Fact]
        public async Task GetSomeIntTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn=await eph.GetOpenConnection();
            var data = new List<UserAutokey>();
            for (var f = 0; f < 10; f++)
            {
                var c = new UserAutokey() { Name = "Hola"+f.ToString() };
                var r1 = conn.Insert(c);
                // asertar que es mayor que 0
                Assert.True((int)r1 > 0);
                data.Add(c);
            }
            var col = data.Select(d => d.Id).ToList();
            var col2 = conn.GetSome<UserAutokey>(col);
            Assert.Equal(col, col2.Select(c => c.Id));
            Assert.Equal(data,col2);
        }

        [Fact]
        public async Task GetSomeStringTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn =await eph.GetOpenConnection();
            var data = new List<CarExplicitKey>();
            for (var f = 0; f < 10; f++)
            {
                var c = new CarExplicitKey() { Id = $"Key{f}", Model = "Hola" + f.ToString() };
                var r1 = conn.Insert(c);
                var check = conn.Get<CarExplicitKey>($"Key{f}");
                Assert.NotNull(check);
                Assert.Equal($"Key{f}", check.Id);
                Assert.Equal(c, check);
                data.Add(c);
            }
            var col = data.Select(d => d.Id).ToList();
            var col2 = conn.GetSome<CarExplicitKey>(col);
            Assert.Equal(col, col2.Select(c => c.Id));
            Assert.Equal(data, col2);
        }

        [Fact]
        public async Task GetWhereTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn= await eph.GetOpenConnection();
            var dataford = new List<CarAutoKey>();
            for (var f = 0; f < 10; f++)
            {
                var c = new CarAutoKey() {Model = "Ford" };
                var c2 = new CarAutoKey() {Model = "Tesla"};
                var r1 = conn.Insert(c);
                // asertar que es mayor que 0
                Assert.True((int)r1 > 0);
                var r2 = conn.Insert(c2);
                Assert.True((int)r2 > 0);
                dataford.Add(c);
            }
            dataford.Sort((x, y) => x.Id.CompareTo(y.Id));
            var lst1 = conn.GetWhere<CarAutoKey>("Model=@model", new { model = "Ford" }).OrderBy(x => x.Id).ToList();
            Assert.Equal(dataford, lst1);
        }

        [Fact]
        public async Task InsertOrUpdateAutoKeyText()
        {
            await using var eph= new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var c1 = new CarAutoKey() { Id = 0, Model =  "Ford", SubModel = "Focus" };
            var k1 = conn.InsertOrUpdate(c1);
            Assert.Equal(k1,c1.Id);
            var c2=c1 with { Model = "Seat", SubModel = "Leon" };
            var k2 = conn.InsertOrUpdate(c2);
            Assert.Equal(k2, c2.Id);
            Assert.Equal(c1.Id, c2.Id);
        }

        [Fact]
        public async Task UpdateIdValueText()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var c1 = new CarAutoKey() { Id = 0, Model = "Ford", SubModel = "Focus"};
            var k1=conn.Insert(c1);
            Assert.Equal(k1, c1.Id);
            var c2 = c1 with { Model = "Seat", SubModel = "Leon" };
            var r1=conn.UpdateIdValue<CarAutoKey,string>((k1,"Tesla"),"Model");
            Assert.True(r1);
            var cr1=conn.Get<CarAutoKey>(k1);
            Assert.Equal(k1, cr1.Id);
            Assert.Equal("Tesla", cr1.Model);
            Assert.Equal(c1.SubModel, cr1.SubModel);
        }

        // Additional tests to improve coverage of ConnectionHelper
        [Fact]
        public async Task Get_NonExistingKey_ReturnsNull_AutoKey()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var res = conn.Get<UserAutokey>(-9999);
            Assert.Null(res);
        }

        [Fact]
        public async Task Get_NonExistingKey_ReturnsNull_ExplicitKey()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var res = conn.Get<CarExplicitKey>("NoSuchKey");
            Assert.Null(res);
        }

        [Fact]
        public async Task GetSome_WithEmptyList_ReturnsEmptyList()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var emptyInt = conn.GetSome<UserAutokey>(new List<int>());
            Assert.Empty(emptyInt);
            var emptyStr = conn.GetSome<CarExplicitKey>(new List<string>());
            Assert.Empty(emptyStr);
        }

        [Fact]
        public async Task DeleteByKey_NonExistingKey_ReturnsFalse()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var r1 = conn.DeleteByKey<UserAutokey>(999999);
            Assert.False(r1);
            var r2 = conn.DeleteByKey<CarExplicitKey>("NoKeyAtAll");
            Assert.False(r2);
        }

        [Fact]
        public async Task GetWhere_WithNoMatches_ReturnsEmpty()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var c1 = new CarAutoKey() { Model = "Ford" };
            var c2 = new CarAutoKey() { Model = "Ford" };
            conn.Insert(c1);
            conn.Insert(c2);
            var lst = conn.GetWhere<CarAutoKey>("Model=@model", new { model = "BMW" }).ToList();
            Assert.Empty(lst);
        }

        // New tests: GetSlim / GetAllSlim / GetSomeSlim / GetWhereSlim and async counterparts
        [Fact]
        public async Task GetSlimAndAllSlimTests()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var a = new CarAutoKey() { Model = "A", SubModel = "S1" };
            var b = new CarAutoKey() { Model = "B", SubModel = "S2" };
            conn.Insert(a);
            conn.Insert(b);

            var gotA = conn.GetSlim<CarAutoKey>(a.Id);
            Assert.NotNull(gotA);
            Assert.Equal(a.Id, gotA.Id);
            var all = conn.GetAllSlim<CarAutoKey>().ToList();
            Assert.True(all.Count >= 2);
        }

        [Fact]
        public async Task GetSomeSlimAndWhereSlimTests()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var items = new List<CarAutoKey>();
            for (var i=0;i<5;i++)
            {
                var c = new CarAutoKey() { Model = i%2==0?"Even":"Odd" };
                conn.Insert(c);
                items.Add(c);
            }
            var ids = items.Select(x=>x.Id).ToList();
            var some = conn.GetSomeSlim<CarAutoKey>(ids);
            Assert.Equal(ids, some.Select(x=>x.Id).ToList());
            var where = conn.GetWhereSlim<CarAutoKey>("Model=@m", new { m = "Even" });
            Assert.All(where, w => Assert.Equal("Even", w.Model));
        }

        [Fact]
        public async Task GetSomeAsyncAndGetWhereAsyncTests()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var data = new List<UserAutokey>();
            for (var f = 0; f < 4; f++)
            {
                var c = new UserAutokey() { Name = "Async"+f.ToString() };
                conn.Insert(c);
                data.Add(c);
            }
            var ids = data.Select(d=>d.Id).ToList();
            var some = await conn.GetSomeAsync<UserAutokey>(ids);
            Assert.Equal(ids, some.Select(s=>s.Id).ToList());
            var where = await conn.GetWhereAsync<UserAutokey>("Name like @p", new { p = "Async%" });
            Assert.True(where.ToList().Count>0);
        }

        [Fact]
        public async Task InsertOrUpdateAsyncTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var c = new CarAutoKey() { Id = 0, Model = "AsyncIns", SubModel = "S" };
            var k = await conn.InsertOrUpdateAsync(c);
            Assert.Equal(k, c.Id);
            var c2 = c with { Model = "Changed" };
            var k2 = await conn.InsertOrUpdateAsync(c2);
            Assert.Equal(k2, c2.Id);
            Assert.Equal(c.Id, c2.Id);
        }

        [Fact]
        public async Task InsertOrUpdateMerge_ExplicitKeyTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var ca = new CarAutoKey() { Id = 0, Model = "M1", SubModel = "S1" };
            // ensure clean: if exists delete by id (no-op)
            _ = conn.DeleteByKey<CarAutoKey>(ca.Id);
            var res = conn.InsertOrUpdateMerge(ca);
            Assert.True(res > 0);
            var got = conn.Get<CarAutoKey>(res);
            Assert.NotNull(got);
            Assert.Equal(res, got.Id);
        }

        [Fact]
        public async Task UpdateIdValueAsyncTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var c = new CarAutoKey() { Model = "U1" };
            conn.Insert(c);
            var ok = await conn.UpdateIdValueAsync<CarAutoKey, string>((c.Id, "UX"), "Model");
            Assert.True(ok);
            var got = conn.Get<CarAutoKey>(c.Id);
            Assert.Equal("UX", got.Model);
        }

        [Fact]
        public async Task SlimAsyncVariantsTest()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var a = new CarAutoKey() { Model = "SA1" };
            var b = new CarAutoKey() { Model = "SA2" };
            conn.Insert(a);
            conn.Insert(b);
            var ga = await conn.GetSlimAsync<CarAutoKey>(a.Id);
            Assert.NotNull(ga);
            var all = await conn.GetAllSlimAsync<CarAutoKey>();
            Assert.True(all.Any());
            var some = await conn.GetSomeSlimAsync<CarAutoKey>(new List<int> { a.Id, b.Id });
            Assert.Equal(2, some.ToList().Count);
            var where = await conn.GetWhereSlimAsync<CarAutoKey>("Model=@m", new { m = "SA1" });
            Assert.Single(where);
        }

        [Fact]
        public async Task QueryJoinAndPartialTests()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            // insert cars
            var c1 = new CarAutoKey() { Model = "J1" };
            var c2 = new CarAutoKey() { Model = "J2" };
            var c3 = new CarAutoKey() { Model = "J3" };
            conn.Insert(c1); conn.Insert(c2); conn.Insert(c3);
            // insert owners
            var o1 = new CarOwnerPartialkeyInt(){ IdCar = c1.Id, Account = "A", Name = "P1" };
            var o2 = new CarOwnerPartialkeyInt(){ IdCar = c1.Id, Account = "B", Name = "P2" };
            var o3 = new CarOwnerPartialkeyInt(){ IdCar = c2.Id, Account = "C", Name = "P3" };
            conn.Insert(o1); conn.Insert(o2); conn.Insert(o3);

             var sql = "SELECT c.*, o.* FROM Car_AutoKey c LEFT JOIN Car_Owner_PartialKeyInt o ON c.Id = o.IdCar";
             var result = conn.QueryJoin<CarAutoKeyJoin, CarOwnerPartialkeyInt>(sql, null, (car, owner) =>
             {
                 var list = car.Maniobras?.ToList() ?? new List<CarOwnerPartialkeyInt>();
                 list.Add(owner);
                 car.Maniobras = list.ToArray();
             });
             // Expect at least c1 and c2 present
             Assert.True(result.Any(r => r.Id == c1.Id));
             var found = result.First(r => r.Id == c1.Id);
             Assert.True(found.Maniobras.Length >= 2);

             // GetPartial by single key
             var partials = conn.GetPartial<CarOwnerPartialkeyInt>(c1.Id).ToList();
             Assert.Equal(2, partials.Count);
             // GetPartial by two keys (id + account) -> should return single
             var single = conn.GetPartial<CarOwnerPartialkeyInt>(c1.Id, "A");
             // single could be null or an item (method returns T?) GetPartial with two keys returns T? per implementation
             Assert.NotNull(single);
         }

        // keep DeleteByKeyAsync here but partial-key deletion moved to PartialKeyTest
        [Fact]
        public async Task DeleteByKeyAsyncTests()
        {
            await using var eph = new EphemeralSqlDatabase();
            using var conn = await eph.GetOpenConnection();
            var u = new UserAutokey() { Name = "DelAsync" };
            var k = conn.Insert(u);
            var ok = await conn.DeleteByKeyAsync<UserAutokey>(k);
            Assert.True(ok);
            var nok = await conn.DeleteByKeyAsync<UserAutokey>(999999);
            Assert.False(nok);
        }

    }
}
