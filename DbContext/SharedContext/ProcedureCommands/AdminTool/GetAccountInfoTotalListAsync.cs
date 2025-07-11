using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class GetAccountInfoTotalListAsync : ProcBaseModelAsync<List<GetAccountDbResult>, GetAccountDbResult>
{
    private const string _ProcedureName = "dbo.gsp_get_account_info_total_list";
    public GetAccountInfoTotalListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        // No Parameters
    }

    public override async Task<List<GetAccountDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }

    
}