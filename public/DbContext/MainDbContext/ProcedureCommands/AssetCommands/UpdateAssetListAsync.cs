using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.AssetCommands;

public class UpdateAssetListAsync : ProcBaseModelAsync<bool, bool>
{
    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; init; }
        public List<AssetDbResult> AssetList { get; init; }
    }
    private const string _ProcedureName = "dbo.gsp_update_asset_info";
    
    public UpdateAssetListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
        _parameters.Add("@assetList", CustomTableDataHelper.CreateCustomQueryParameter(inParams.AssetList));
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }

    
}