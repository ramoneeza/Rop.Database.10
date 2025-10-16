using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Rop.Database;
using Rop.Database10.Tracking;
using Rop.Database10.TypeHandlers;

namespace Rop.Database10
{
    /// <summary>
    /// Implement a database class from abstract database for sql server
    /// </summary>
    public partial class Database:AbsDatabase
    {
        private readonly Database _parentdatabase;
        private readonly TableDependencyCycleTaskQueue _cycleTasks;
        private readonly ConcurrentDictionary<string,Database> _foreignDatabaseInstances;
        private readonly ConcurrentDictionary<RuntimeTypeHandle,Database> _foreignDatabasesPerType;
        private readonly ConcurrentDictionary<KeyDescription, Database> _foreignDatabasesPerKey;
        public Database(string strconn,Database? parentdatabase=null) : base(strconn)
        {
            _parentdatabase = parentdatabase??this;
            if (parentdatabase == null)
            {
                _cycleTasks = new TableDependencyCycleTaskQueue();
                _foreignDatabasesPerType = new ConcurrentDictionary<RuntimeTypeHandle, Database>();
                _foreignDatabasesPerKey = new ConcurrentDictionary<KeyDescription, Database>();
                _foreignDatabaseInstances = new ConcurrentDictionary<string, Database>(StringComparer.OrdinalIgnoreCase);
                _foreignDatabaseInstances[this.MainDatabaseName] = this;
                NewTypeHandlers.Ensure();
            }
            else
            {
                _cycleTasks = parentdatabase._cycleTasks;
                _foreignDatabaseInstances = parentdatabase._foreignDatabaseInstances;
                _foreignDatabasesPerType = parentdatabase._foreignDatabasesPerType;
                _foreignDatabasesPerKey = parentdatabase._foreignDatabasesPerKey;
                _foreignDatabaseInstances.TryAdd(this.MainDatabaseName,this);
            }
            if (!_parentdatabase.IsParentDataBase) throw new ArgumentException("ParentDatabase is no parent");
        }
        public bool IsParentDataBase => _parentdatabase == this;
        public override DbConnection FactoryConnection() => new SqlConnection(Strconn);
        public bool IsForeingTable(KeyDescription td)
        {
            return td.IsForeignTable && !td.ForeignDatabaseName.Equals(this.MainDatabaseName, StringComparison.OrdinalIgnoreCase);
        }
        public bool IsForeingTable<T>() where T:class
        {
            var td = GetKeyDescription<T>() ?? throw new Exception($"Table for {typeof(T).Name} not found");
            return IsForeingTable(td);
        }
        public Database FactoryExternalDatabase(string databasename)
        {
            if (_foreignDatabaseInstances.TryGetValue(databasename, out var db)) return db;
            var strconn = Strconn.Replace(this.MainDatabaseName, databasename);
            db=new Database(strconn,this._parentdatabase);
            return db;
        }
        public Database FactoryExternalDatabase(KeyDescription td)
        {
            if (_foreignDatabasesPerKey.TryGetValue(td, out var db)) return db;
            if (!IsForeingTable(td))
            {
                db = this;
                _foreignDatabasesPerKey[td] = db;
                return this;
            }
            db = FactoryExternalDatabase(td.ForeignDatabaseName);
            _foreignDatabasesPerKey[td] = db;
            return db;
        }
        public Database FactoryExternalDatabase<T>() where T: class
        {
            var td = GetKeyDescription<T>() ?? throw new Exception($"Table for {typeof(T).Name} not found");
            return FactoryExternalDatabase(td);
        }
        public DbConnection FactoryConnection(KeyDescription td)
        {
            var db=FactoryExternalDatabase(td);
            return new SqlConnection(db.Strconn);
        }
        public DbConnection FactoryConnection<T>() where T:class
        {
            var td = GetKeyDescription<T>()?? throw new Exception($"Table for {typeof(T).Name} not found");
            return FactoryConnection(td);
        }
        public Database FactoryExternalDatabase(string databasename,string? server)
        {
            if (string.IsNullOrEmpty(server) || server.Equals(this.Server, StringComparison.OrdinalIgnoreCase))
            {
                return FactoryExternalDatabase(databasename);
            }
            var strconn = Strconn.Replace(this.MainDatabaseName, databasename).Replace(this.Server, server);
            return new Database(strconn, null);
        }
        public SqlTableDependency GetTableDependency(Type type, int changesPriority)
        {
            var key = SqlTableDependency.CalcKey(this, type);
            return _cycleTasks.TryAdd(key, _ => new SqlTableDependency(this, type, changesPriority));
        }
    }
}
