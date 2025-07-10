using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.InventoryCommands;

public class GetInventoryListAsync : ProcBaseModelAsync<List<InventoryDbResult>, InventoryDbResult>
{
    private const string _ProcedureName = "dbo.gsp_get_inventory_list";
    
    public GetInventoryListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(long accountId)
    {
        _parameters.Add("@accountId", accountId);
    }

    public override async Task<List<InventoryDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }
}