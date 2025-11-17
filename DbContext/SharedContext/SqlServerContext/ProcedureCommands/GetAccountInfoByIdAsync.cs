using DbContext.Common;
using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.SqlServerContext.ProcedureCommands;

public class GetAccountInfoByIdAsync(DapperServiceBase dbContext, SqlTransaction transaction = null)
    : ProcBaseModelAsync<GetAccountDbResult, GetAccountDbResult>(dbContext, _ProcedureName, transaction)
{
    public struct InParameters : IDbInParameters
    {
        public string LoginId  { get; init; }
    }
    private const string _ProcedureName = "dbo.gsp_get_account_info_by_login_id";

    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@loginId", inParams.LoginId);
    }

    public override async Task<GetAccountDbResult> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }

    
}

