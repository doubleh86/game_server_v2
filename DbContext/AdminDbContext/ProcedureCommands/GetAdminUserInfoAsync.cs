using System.Data;
using DbContext.AdminDbContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.AdminDbContext.ProcedureCommands;

public class GetAdminUserInfoAsync : ProcBaseModelAsync<AdminUserDbModel, AdminUserDbModel>
{
    private const string _ProcedureName = "dbo.gsp_get_admin_info";
    public GetAdminUserInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(string userId)
    {
        _parameters.Add("@loginId", userId, dbType: DbType.String, size: 100);
    }

    public override async Task<AdminUserDbModel> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }
    
    
}