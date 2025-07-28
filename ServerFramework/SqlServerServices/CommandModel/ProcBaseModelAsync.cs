using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.SqlServerServices.CommandModel;

public abstract class ProcBaseModelAsync<TResult, TDbModel>
{
    private const int _ProcedureError = -1;

    private readonly DapperServiceBase _dbContext;
    private readonly string _procedureName;
    protected readonly DynamicParameters _parameters;
    private readonly SqlTransaction _transaction;
    public abstract void SetParameters(IDbInParameters inParameters);
    public abstract Task<TResult> ExecuteProcedureAsync();

    protected ProcBaseModelAsync(DapperServiceBase dbContext, string procedureName, SqlTransaction transaction = null)
    {
        _procedureName = procedureName;
        _dbContext = dbContext;
        _transaction = transaction;
        
        _parameters = new DynamicParameters();
        _SetReturnParameter();
    }
    
    protected virtual int _GetResultCode()
    {
        return _parameters?.Get<int>("@Result") ?? _ProcedureError;
    }
    
    protected virtual async Task<SqlMapper.GridReader> _RunDbProcedureMultipleResultAsync()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return await _dbContext.ExecuteProcedureMultipleSelectAsync(_procedureName, _parameters, transaction: _transaction);
    }

    protected virtual async Task<IEnumerable<dynamic>> _RunDbProcedureReturnDynamicAsync()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return await _dbContext.ExecuteProcedureDynamicAsync(_procedureName, _parameters, transaction: _transaction);
    }

    protected virtual async Task<IEnumerable<TDbModel>> _RunDbProcedureReturnModelAsync()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return await _dbContext.ExecuteProcedureWithModelAsync<TDbModel>(_procedureName, _parameters, transaction: _transaction);
    }

    protected virtual void _CheckExceptionError()
    {
        switch (_GetResultCode())
        {
            case _ProcedureError:
                throw new DatabaseException(ServerError.DbProcedureError, $"[{_procedureName}]|[{_GetResultCode()}] error");

        }
    }

    private void _SetReturnParameter()
    {
        _parameters?.Add("@Result", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
    }
}