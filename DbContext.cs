using SQLitePCL;
using System.Data;

namespace SqlLiteExplorer;

public class DbContext
{
    private string? _connectionString;
    public required string ConnectionString
    {
        get => _connectionString!;
        set {
            if(string.IsNullOrWhiteSpace(value) )
                throw new DataException("Connection string cannot be null or empty.");

            _connectionString = value;
        }
    }

    static DbContext()
    {
        //this must be done once to allow all sqlite connections
        Batteries.Init();
    }

    public SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public async Task<bool> TestConnection()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public SqliteCommand GetCommand(string sql, ICollection<SqliteParameter>? parameters = null)
    {
        SqliteConnection connection = GetConnection();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;

        if (parameters is not null)
            command.Parameters.AddRange(parameters);
        return command;
    }

    public async Task<int> ExecuteNonQuery(string sql, ICollection<SqliteParameter>? parameters = null)
    {
        using SqliteCommand command = GetCommand(sql, parameters);
        int rowsAffected = await command.ExecuteNonQueryAsync();

        command.Connection?.Dispose();
        return rowsAffected;
    }

    public async Task<T?> QueryScalar<T>(string sql, ICollection<SqliteParameter>? parameters = null)
    {
        using SqliteCommand command = GetCommand(sql,parameters);
        var value = (T?)(await command.ExecuteScalarAsync());

        command.Connection?.Dispose();
        return value;
    }

    public async Task<List<T>> QuerySingleField<T>(string sql, int index = 0, ICollection<SqliteParameter>? parameters = null)
    {
        using SqliteCommand command = GetCommand(sql,parameters);

        List<T> results = [];
        using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows) return [];

        while (reader.Read())
        {
            T result = (T)reader[index];
            results.Add(result);
        }

        command.Connection?.Dispose();
        return results;
    }

    #region Generic table functions

    public async Task<int> TruncateTable(string tableName)
    {
        return await ExecuteNonQuery($"DELETE FROM [{tableName}] RETURNING *");
    }

    public async Task<long> GetRecordsCount(string tableName)
    {
        return await QueryScalar<long>($"SELECT COUNT(*) FROM [{tableName}]");
    }

    public async Task<List<string>> GetTableNames()
    {
        return await QuerySingleField<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;");
    }

    public async Task<bool> TableExists(string tableName)
    {
        long? result = await QueryScalar<long?>($"SELECT 1 FROM sqlite_master WHERE type='table' AND name='{tableName}' ");
        return result == 1;
    }

    #endregion

}
