using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace ServerFramework.SqlServerServices.CommandModel;

public abstract class ProcBaseModel<TResult, TDbModel>
{
    protected const int _ResultOk = 0;
    private const int _ProcedureError = -1;

    protected readonly DapperServiceBase _dbContext;
    protected readonly string _procedureName;
    protected DynamicParameters _parameters;
    private readonly SqlTransaction _transaction;
    
    public abstract TResult ExecuteProcedure();

    protected ProcBaseModel(DapperServiceBase dbContext, string procedureName, SqlTransaction transaction = null)
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

    protected virtual SqlMapper.GridReader _RunDbProcedureMultipleResult()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return _dbContext.ExecuteProcedureMultipleSelect(_procedureName, _parameters, transaction: _transaction);
    }

    protected virtual IEnumerable<dynamic> _RunDbProcedureReturnDynamic()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return _dbContext.ExecuteProcedureDynamic(_procedureName, _parameters, transaction: _transaction);
    }
    
    protected virtual IEnumerable<TDbModel> _RunDbProcedureReturnModel()
    {
        if (_parameters == null)
            throw new DatabaseException(ServerError.DbError, $"[{_procedureName}]|[Parameters is null]");

        return _dbContext.ExecuteProcedureWithModel<TDbModel>(_procedureName, _parameters, transaction: _transaction);
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