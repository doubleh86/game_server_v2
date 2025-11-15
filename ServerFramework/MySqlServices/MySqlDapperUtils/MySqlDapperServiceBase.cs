using System.Data;
using Dapper;
using MySqlConnector;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.MySqlServices.MySqlDapperUtils;

public abstract class MySqlDapperServiceBase : IDisposable, IDapperService
{
    private MySqlConnection _connection;
    private readonly SqlServerDbInfo _serverInfo;
    private string _connectionString;
    
    public MySqlConnection Connection => _connection;

    protected MySqlDapperServiceBase(SqlServerDbInfo dbInfo)
    {
        _serverInfo = dbInfo;
        _InitializeConnectionString();
    }

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
        _connectionString = $"Server={_serverInfo.Ip};Port={_serverInfo.Port};" +
                            $"Database={_serverInfo.DatabaseName};Uid={_serverInfo.UserId};" +
                            $"Pwd={_serverInfo.Password};";
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
    }


}