using DbContext.Common;
using DbContext.Common.Models;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.AssetCommands;

public class GetAssetInfoAsync : ProcBaseModelAsync<List<AssetDbResult>, AssetDbResult>
{
    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; init; }    
    }
    
    private const string _ProcedureName = "dbo.gsp_get_asset_info";
    public GetAssetInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
    }

    public override async Task<List<AssetDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }

}