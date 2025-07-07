using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.SqlServerServices.DapperUtils;

/// <summary>
///  using var connection = _GetConnection();
///  커넥션 호출 시에는 using 을 써서 항상 쿼리 후 항상 Close() 하도록 한다.
///   -> Pool 관리는 드라이버에서 하므로 연결을 유지 할 필요가 없다.
///   -> 가능 하면 하나의 using 블럭에서 하나의 Query 또는 Procedure를 호출한다.
///
/// => Next 작업
///    => 비동기 쿼리 메서드
///    => using var connection = _GetConnection() 호출 관련하여 좀 더 명확하게 처리할 수 있게
///
/// </summary>
public abstract class DapperServiceBase : IDisposable, ISqlServerContext
{
    private SqlConnection _connectionV2;
    private readonly SqlServerDbInfo _serverInfo;
    private string _connectionString;
    
    protected DapperServiceBase(SqlServerDbInfo serverInfo)
    {
        _serverInfo = serverInfo;
        _InitializeConnectionString();
    }

    private void _InitializeConnectionString()
    {
        var maxPoolSize = _serverInfo.MaxPoolSize < 100 ? 100 : _serverInfo.MaxPoolSize;
        _connectionString = $"server={_serverInfo.Ip},{_serverInfo.Port};uid={_serverInfo.UserId};" +
                            $"pwd={_serverInfo.Password};database={_serverInfo.DatabaseName};max pool size={maxPoolSize};Encrypt=false";
    }

    protected SqlConnection _GetConnection()
    {
        if (_connectionV2 != null && _connectionV2.State == ConnectionState.Open)
            return _connectionV2;

        _connectionV2 = new SqlConnection(_connectionString);
        return _connectionV2;
    }

    public IEnumerable<TDbModel> ExecuteProcedureWithModel<TDbModel>(string procedureName, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        return _connectionV2.Query<TDbModel>(procedureName, param, commandType: commandType, transaction:transaction);
    }
    
    public async Task<IEnumerable<TDbModel>> ExecuteProcedureWithModelAsync<TDbModel>(string procedureName, object param, SqlTransaction transaction = null,CommandType commandType = CommandType.StoredProcedure)
    {
        return await _connectionV2.QueryAsync<TDbModel>(procedureName, param, commandType: commandType, transaction:transaction);
    }
    
    public IEnumerable<dynamic> ExecuteProcedureDynamic(string procedureName, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        return _connectionV2.Query(procedureName, param, commandType: commandType, transaction:transaction);
    }
    
    public async Task<IEnumerable<dynamic>> ExecuteProcedureDynamicAsync(string procedureName, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        return await _connectionV2.QueryAsync(procedureName, param, commandType: commandType, transaction:transaction);
    }

    public SqlMapper.GridReader ExecuteProcedureMultipleSelect(string procedureName, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        return _connectionV2.QueryMultiple(procedureName, param, commandType: commandType, transaction:transaction);
    }
    
    public async Task<SqlMapper.GridReader> ExecuteProcedureMultipleSelectAsync(string procedureName, object param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        return await _connectionV2.QueryMultipleAsync(procedureName, param, commandType: commandType, transaction:transaction);
    }


    /// <summary>
    /// Row 가 변경 될 시에만 사용 한다.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="param"></param>
    /// <param name="transaction"></param>
    /// <param name="commandType"></param>
    /// <returns>변경된 Row 값 ( 없으면 -1 )</returns>
    /// <exception cref="DatabaseException"></exception>
    public int ExecuteUpdateStoredProcedure(string procedureName, DynamicParameters param, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure)
    {
        try
        {
            return _connectionV2.Execute(procedureName, param, commandType: commandType, transaction:transaction);
        }
        catch (Exception e)
        {
            throw new DatabaseException(ServerError.DbError, e.Message);
        }
    }

    public void Dispose()
    {
        _connectionV2?.Close();
        _connectionV2?.Dispose();
    }

    
}