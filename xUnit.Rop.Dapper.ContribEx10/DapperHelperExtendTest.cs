using Dapper;
using Rop.Dapper.ContribEx10;
using System.Data;
using xUnit.Rop.Dapper.ContribEx10.Data;
using System.Linq.Expressions;

namespace xUnit.Rop.Dapper.ContribEx10
{
    public partial class DapperHelperExtendTest
    {
        public DapperHelperExtendTest()
        {
        }
        
        [Fact]
        public void ExplicitKeyPropertiesCacheTest()
        {
            // No explicitKey
            var explicitkeys = DapperHelperExtend.ExplicitKeyPropertiesCache(typeof(CarAutoKey));
            Assert.Empty(explicitkeys);
            // Single explicitKey
            explicitkeys=DapperHelperExtend.ExplicitKeyPropertiesCache(typeof(CarExplicitKey));
            Assert.Single(explicitkeys);
            // Check Twice to check cache
            explicitkeys=DapperHelperExtend.ExplicitKeyPropertiesCache(typeof(CarExplicitKey));
            Assert.Single(explicitkeys);
            // Double explicit Key
            explicitkeys=DapperHelperExtend.ExplicitKeyPropertiesCache(typeof(CarOwnerPartialkeyInt));
            Assert.Empty(explicitkeys);
        }
        [Fact]
        public void KeyPropertiesCacheTest()
        {
            //Single Key
            var autokeys = DapperHelperExtend.KeyPropertiesCache(typeof(CarAutoKey));
            Assert.Single(autokeys);
            // Check Twice to check cache
            autokeys=DapperHelperExtend.KeyPropertiesCache(typeof(CarAutoKey));
            Assert.Single(autokeys);
            // No autoKey
            autokeys=DapperHelperExtend.KeyPropertiesCache(typeof(CarExplicitKey));
            Assert.Empty(autokeys);
            // Double Key
            var partialkey=DapperHelperExtend.GetPartialKeyDescription<CarOwnerPartialkeyInt>();
            Assert.NotNull(partialkey);
        }

        [Fact]
        public void GetTableNameTest()
        {
            var tablename = DapperHelperExtend.GetTableName(typeof(CarAutoKey));
            Assert.Equal("Car_AutoKey",tablename);
        }
        [Fact]
        public void TypePropertiesCacheTest()
        {
            var properties = DapperHelperExtend.TypePropertiesCache(typeof(CarAutoKey));
            Assert.Equal(new[] {"Id","Model","SubModel"}, properties.Select(p=>p.Name).ToArray());
        }
        [Fact]
        public void ComputedPropertiesCacheTest()
        {
            var properties = DapperHelperExtend.ComputedPropertiesCache(typeof(CarAutoKeyJoin));
            Assert.Equal(new[] {"Maniobras"}, properties.Select(p=>p.Name).ToArray());
            properties = DapperHelperExtend.ComputedPropertiesCache(typeof(CarAutoKey));
            Assert.Empty(properties);
        }
        [Fact]
        public async Task GetFormatter_TrivialCheck()
        {
            await using var eph= new EphemeralSqlDatabase();
            using var conn=await eph.GetOpenConnection();
            var formatter = DapperHelperExtend.GetFormatter(conn);
            Assert.NotNull(formatter);
        }
        [Fact]
        public void SelectGetCacheTest()
        {
            var select = DapperHelperExtend.SelectGetCache(typeof(CarAutoKey));
            Assert.Equal("select * from Car_AutoKey where Id = @id",select);
        }
        [Fact]
        public void SelectGetAllCacheTest()
        {
            var select = DapperHelperExtend.SelectGetAllCache(typeof(CarAutoKey));
            Assert.Equal("select * from Car_AutoKey",select);
        }
        [Fact]
        public void GetDeleteByKeyCacheTest()
        {
            var select = DapperHelperExtend.DeleteByKeyCache(typeof(CarAutoKey));
            Assert.Equal("DELETE FROM Car_AutoKey WHERE Id = @id",select);
        }
        // Class / Table / Key info

        [Fact]
        public void GetSingleKeyTest()
        {
            var (propkey,isautokey) = DapperHelperExtend.GetSingleKey(typeof(CarAutoKey));
            Assert.Equal("Id",propkey.Name);
            Assert.True(isautokey);
            // Explicit
            (propkey,isautokey) = DapperHelperExtend.GetSingleKey(typeof(CarExplicitKey));
            Assert.Equal("Id",propkey.Name);
            Assert.False(isautokey);
        }
        [Fact]
        public void GetKeyDescriptionTest1()
        {
            var keyDescription = DapperHelperExtend.GetKeyDescription(typeof(CarAutoKey));
            Assert.Equal("Car_AutoKey",keyDescription.TableName);
            Assert.Equal("Id",keyDescription.KeyName);
            Assert.NotNull(keyDescription.KeyProp);
            Assert.True(keyDescription.IsAutoKey);
            Assert.False(keyDescription.KeyTypeIsString);
        }
        [Fact]
        public void GetKeyDescriptionTest2()
        {
            var keyDescription = DapperHelperExtend.GetKeyDescription(typeof(CarExplicitKey));
            Assert.Equal("Car_ExplicitKey",keyDescription.TableName);
            Assert.Equal("Id",keyDescription.KeyName);
            Assert.NotNull(keyDescription.KeyProp);
            Assert.False(keyDescription.IsAutoKey);
            Assert.True(keyDescription.KeyTypeIsString);
        }
        [Fact]
        public void GetKeyDescriptionAndValueTest1()
        {
            var item = new CarAutoKey() { Id = 1 };
            var (keyDescription,value) = DapperHelperExtend.GetKeyDescriptionAndValue(item);
            Assert.Equal("Car_AutoKey",keyDescription.TableName);
            Assert.Equal("Id",keyDescription.KeyName);
            Assert.True(keyDescription.IsAutoKey);
            Assert.False(keyDescription.KeyTypeIsString);
            Assert.Equal(1,value);
        }
        [Fact]
        public void GetKeyDescriptionAndValueTest2()
        {
            var item = new CarExplicitKey(){Id="MyKey"};
            var (keyDescription,value) = DapperHelperExtend.GetKeyDescriptionAndValue(item);
            Assert.Equal("Car_ExplicitKey",keyDescription.TableName);
            Assert.Equal("Id",keyDescription.KeyName);
            Assert.NotNull(keyDescription.KeyProp);
            Assert.False(keyDescription.IsAutoKey);
            Assert.True(keyDescription.KeyTypeIsString);
            Assert.Equal("MyKey",value);
        }
        [Fact]
        public void GetKeyValueTest()
        {
            var item1 = new CarAutoKey() { Id = 1 };
            var item2 = new CarExplicitKey() { Id = "MyKey" };
            var value1 = DapperHelperExtend.GetKeyValue(item1);
            var value2 = DapperHelperExtend.GetKeyValue(item2);
            Assert.Equal(1,value1);
            Assert.Equal("MyKey",value2);
        }
        [Fact]
        public void SetKeyValueTest()
        {
            var item1 = new CarAutoKey();
            var item2 = new CarExplicitKey();
            DapperHelperExtend.SetKeyValue(item1,2);
            DapperHelperExtend.SetKeyValue(item2,"OtherKey");
            Assert.Equal(2,item1.Id);
            Assert.Equal("OtherKey",item2.Id);
        }

        // IdLists
        [Fact]
        public void GetIdListTest()
        {
            var lst1 = DapperHelperExtend.GetIdList(new[] {1,2,3,4,5,6});
            var lst2 = DapperHelperExtend.GetIdList(new[] {"A","B","C","D"});
            var lst3 = DapperHelperExtend.GetIdListDyn(new object[]{"A","B"});
            var lst4 = DapperHelperExtend.GetIdListDyn(new object[]{1,2});
            Assert.Equal("1,2,3,4,5,6",lst1);
            Assert.Equal("'A','B','C','D'",lst2);
            Assert.Equal("'A','B'",lst3);
            Assert.Equal("1,2",lst4);
        }
        // ExternalDatabase tests
        [Fact]
        public void GetExternalDatabaseName()
        {
            var name1 = DapperHelperExtend.GetForeignDatabaseName(typeof(ExtWithAttTable));
            var name2 = DapperHelperExtend.GetForeignDatabaseName(typeof(ExtWithAttTable2));
            var name3 = DapperHelperExtend.GetForeignDatabaseName(typeof(ExtWithKey1));
            var name4 = DapperHelperExtend.GetForeignDatabaseName(typeof(ExtWithKey2));
            var desired = "Intranet";
            Assert.Equal(desired,name1);
            Assert.Equal(desired,name2);
            Assert.Equal(desired,name3);
            Assert.Equal(desired,name4);
        }

        [Fact]
        public void GetMemberName_Tests()
        {
            Expression<Func<CarAutoKey, object>> ex1 = c => c.Model;
            var name1 = ex1.GetMemberName();
            Assert.Equal("Model", name1);

            Expression<Func<CarAutoKey, object>> ex2 = c => (object)c.Id;
            var name2 = ex2.GetMemberName();
            Assert.Equal("Id", name2);
        }

        // Keep tests that relate to caches/select SQL generation
        [Fact]
        public void SelectGetPartialCaches_ReturnsSql()
        {
            var s1 = DapperHelperExtend.SelectGetPartialCache(typeof(CarOwnerPartialkeyInt));
            Assert.False(string.IsNullOrEmpty(s1));
            Assert.Contains("SELECT", s1, System.StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Car_Owner_PartialKeyInt", s1);

            var s2 = DapperHelperExtend.SelectGetPartial2Cache(typeof(CarOwnerPartialkeyInt));
            Assert.False(string.IsNullOrEmpty(s2));
            Assert.Contains("SELECT", s2, System.StringComparison.OrdinalIgnoreCase);

            var s12 = DapperHelperExtend.SelectGetPartial12Cache(typeof(CarOwnerPartialkeyInt));
            Assert.False(string.IsNullOrEmpty(s12));
            Assert.Contains("WHERE", s12, System.StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PartialKeyValueAndDescriptionTests()
        {
            var item = new CarOwnerPartialkeyInt() { IdCar = 123, Account = "ACC", Name = "N" };
            var v1 = DapperHelperExtend.GetPartialKeyValue(item);
            var v2 = DapperHelperExtend.GetPartialKey2Value(item);
            Assert.Equal(123, v1);
            Assert.Equal("ACC", v2);

            var pd = DapperHelperExtend.GetPartialKeyDescription(typeof(CarOwnerPartialkeyInt));
            Assert.NotNull(pd);
            var (desc, key) = DapperHelperExtend.GetPartialKeyDescriptionAndValue(item);
            Assert.Equal(pd.TableName, desc.TableName);
            Assert.Equal(123, key);
        }

    }
}
