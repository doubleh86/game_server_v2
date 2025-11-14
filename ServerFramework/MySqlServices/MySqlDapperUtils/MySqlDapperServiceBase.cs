using System.Data;
using Dapper;
using MySqlConnector;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.MySqlServices.MySqlDapperUtils;

public abstract class MySqlDapperServiceBase(SqlServerDbInfo dbInfo) : IDisposable, IDapperService
{
    private MySqlConnection _connection;
    private string _connectionString;
    
    public MySqlConnection Connection => _connection;

    protected MySqlConnection _GetConnection()
    {
        if(_connection != null && _connection.State == ConnectionState.Open)
            return _connection;
        
        _connection = new MySqlConnection(_connectionString);
        return _connection;
    }

    public async Task<IEnumerable<TDbModel>> ExecuteQueryWithModelAsync<TDbModel>(string sql, object param = null,
                                                                                  MySqlTransaction transaction = null)
    {
        return await _connection.QueryAsync<TDbModel>(sql, param, transaction);
    }

    public async Task<IEnumerable<dynamic>> ExecuteQueryDynamicAsync(string sql, object param = null, MySqlTransaction transaction = null)
    {
        return await _connection.QueryAsync(sql, param, transaction);
    }
    
    public void _InitializeConnectionString()
    {
        _connectionString = $"Server={dbInfo.Ip};Port={dbInfo.Port};" +
                            $"Database={dbInfo.DatabaseName};Uid={dbInfo.UserId};" +
                            $"Pwd={dbInfo.Password};";
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }


}