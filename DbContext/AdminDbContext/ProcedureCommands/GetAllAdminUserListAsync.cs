using DbContext.AdminDbContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.AdminDbContext.ProcedureCommands;

public class GetAllAdminUserListAsync : ProcBaseModelAsync<List<AdminUserDbModel>, AdminUserDbModel>
{
    private const string _ProcedureName = "dbo.gsp_get_all_admin_info";
    public GetAllAdminUserListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override async Task<List<AdminUserDbModel>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }
}