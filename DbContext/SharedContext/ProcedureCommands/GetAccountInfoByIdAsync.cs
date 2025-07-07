using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands;

public class GetAccountInfoByIdAsync : ProcBaseModelAsync<GetAccountDbResult, GetAccountDbResult> 
{
    private const string _ProcedureName = "dbo.gsp_get_account_info_by_login_id";
    public GetAccountInfoByIdAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(string loginId)
    {
        _parameters.Add("@loginId", loginId);
    }

    public override async Task<GetAccountDbResult> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }
}

