using System.Data;
using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class GetAccountInfoListAsync : ProcBaseModelAsync<List<GetAccountDbResult>, GetAccountDbResult>
{
    private const string _ProcedureName = "dbo.gsp_get_account_info_list";
    public GetAccountInfoListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(string searchValue)
    {
        _parameters.Add("@search", searchValue, dbType:DbType.String, size: 50);
    }

    public override async Task<List<GetAccountDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }
}