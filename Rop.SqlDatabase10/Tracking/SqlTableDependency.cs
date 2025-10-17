using System.Diagnostics;
using Rop.Database10.Tracking;

namespace Rop.Database10
{
    
    public class SqlTableDependency:ICycleTask
    {
        public string Key { get; }
        public TimeSpan Interval { get; }
        public Database Database { get; }
        public KeyDescription TableDescription { get; }
        public ChangeTrackingPriority Priority { get;  }
        public long TableVersion { get; private set; }
        public event EventHandler<DeltaChanges>? OnChanged;

        // Cambiado a internal para que Database (mismo ensamblado) pueda instanciarlo.
        internal SqlTableDependency(Database database, Type t, ChangeTrackingPriority priority = ChangeTrackingPriority.Default)
        {
            var name = CalcKey(database, t);
            Key = name;
            Priority = priority;
            if ((int)Priority > 10) throw new ArgumentException("Priority from 0 (maximum) to 10 (minimum)");
            Interval =Priority.ToInterval();
            TableDescription = DapperHelperExtend.GetAnyKeyDescription(t) ?? throw new ArgumentException($"type {t} has not any kind of key");
            Database = database.FactoryExternalDatabase(TableDescription);
            TableVersion = Database.GetDbVersion(TableDescription).ValueOrThrow();
        }
        public string? PayLoad()
        {
            using var sqlConnection = Database.FactoryConnection();
            try
            {
                sqlConnection.Open();
                var r = sqlConnection.GetTableChanges(TableVersion, TableDescription);
                var deltaChanges = r.ValueOrThrow();
                if (deltaChanges is { IsNewVersion: true, IsEmpty: false })
                {
                    TableVersion = deltaChanges.Version;
                    _sendChanges(deltaChanges);
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return ex.Message;
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void _sendChanges(DeltaChanges deltaChanges)
        {
            var handler = OnChanged;
            if (handler == null) return;

            _ = Task.Run(() =>
            {
                foreach (var d in handler.GetInvocationList())
                {
                    if (d is not EventHandler<DeltaChanges> h) continue;
                    try
                    {
                        h.Invoke(this, deltaChanges);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
            });
        }

        public static string CalcKey(Database database, Type t)
        {
            var td = database.GetAnyKeyDescription(t) ?? throw new ArgumentException($"type {t} has not any kind of key");
            var name = database.MainDatabaseName + "." + td.TableName;
            return name;
        }
    }
}