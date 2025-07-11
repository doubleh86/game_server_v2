using System.Data;
using DbContext.Common;
using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class GetAccountInfoListAsync : ProcBaseModelAsync<List<GetAccountDbResult>, GetAccountDbResult>
{
    public struct InParameters : IDbInParameters
    {
        public string SearchValue { get; init; }
    }
    
    private const string _ProcedureName = "dbo.gsp_get_account_info_list";
    public GetAccountInfoListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@search", inParams.SearchValue, dbType:DbType.String, size: 50);
    }

    public override async Task<List<GetAccountDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }

    
}