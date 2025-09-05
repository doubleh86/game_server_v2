using System.Data;
using DbContext.AdminDbContext.DbResultModel;
using DbContext.Common;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.AdminDbContext.ProcedureCommands;

public class GetAdminUserInfoAsync : ProcBaseModelAsync<AdminUserDbModel, AdminUserDbModel>
{
    public struct InParameters : IDbInParameters
    {
        public string UserId { get; set; }
    }
    private const string _ProcedureName = "dbo.gsp_get_admin_info";
    public GetAdminUserInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@loginId", inParams.UserId, dbType: DbType.String, size: 100);
    }

    public override async Task<AdminUserDbModel> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }

    
}