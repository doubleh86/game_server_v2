using System.Data;
using DbContext.AdminDbContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.AdminDbContext.ProcedureCommands;

public class CreateNewAdminUserAsync : ProcBaseModelAsync<AdminUserDbModel, AdminUserDbModel>
{
    private const string _ProcedureName = "dbo.gsp_create_new_admin_user";
    public CreateNewAdminUserAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(string userId, string password, int adminType)
    {
        _parameters.Add("@loginId", userId, dbType:DbType.String, size: 100);
        _parameters.Add("@password", password, dbType:DbType.String, size: 100);
        _parameters.Add("@adminType", adminType);
    }

    public override async Task<AdminUserDbModel> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }
}