using Microsoft.Data.SqlClient;
using System.Net.NetworkInformation;
using System;
using System.Data;
using System.Threading.Tasks;

namespace xUnit.Rop.Dapper.ContribEx10;

public sealed class EphemeralSqlDatabase : IAsyncDisposable
{
    public string ConnectionString { get; }
    private readonly string _dbName;
    private volatile bool _initialized;
    public EphemeralSqlDatabase()
    {
        _dbName = "TestDb_" + Guid.NewGuid().ToString("N");
        ConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database={_dbName}";
    }

    public async Task InitializeAsync()
    {
        using var masterConn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;");
        await masterConn.OpenAsync();

        // Crear base vacía
        using (var cmd = masterConn.CreateCommand())
        {
            cmd.CommandText = $"CREATE DATABASE [{_dbName}]";
            await cmd.ExecuteNonQueryAsync();
        }

        // Crear tablas y esquema
        using var conn = new SqlConnection(ConnectionString);
        await conn.OpenAsync();
        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = @"
            CREATE TABLE User_AutoKey (
                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                Surname VARCHAR(100) NOT NULL,
                Created DATETIME2
            );
            CREATE TABLE Car_ExplicitKey(
               Id VARCHAR(50) NOT NULL PRIMARY KEY,
               Model VARCHAR(50) NOT NULL,
               SubModel VARCHAR(50) NOT NULL
            );
            CREATE TABLE Car_AutoKey(
               Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
               Model VARCHAR(50) NOT NULL,
               SubModel VARCHAR(50) NOT NULL
            );
            CREATE TABLE Car_Owner_PartialKeyStr(
               IdCar VARCHAR(50) NOT NULL,
               Account VARCHAR(50) NOT NULL,
               Name VARCHAR(50),
               CONSTRAINT PK_MiTabla PRIMARY KEY (IdCar,Account)
            );
            CREATE TABLE Car_Owner_PartialKeyInt(
               IdCar INT NOT NULL,
               Account VARCHAR(50) NOT NULL,
               Name VARCHAR(50),
                CONSTRAINT PK_MiTabla2 PRIMARY KEY (IdCar,Account)
            );
        ";
        await cmd2.ExecuteNonQueryAsync();
        _initialized = true;
    }
    public async Task<IDbConnection> GetOpenConnection()
    {
        if (!_initialized)
            await InitializeAsync();
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            using var masterConn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;");
            await masterConn.OpenAsync();

            using var cmd = masterConn.CreateCommand();
            cmd.CommandText = $@"
                ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{_dbName}];
            ";
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // Ignorar si ya fue eliminada
        }
    }
}

