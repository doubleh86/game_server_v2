using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.InventoryCommands;

public class UpdateInventoryListAsync : ProcBaseModelAsync<bool, bool>
{
    private const string _ProcedureName = "dbo.gsp_update_item_info";
    public UpdateInventoryListAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(long accountId, List<InventoryDbResult> itemList)
    {
        _parameters.Add("@accountId", accountId);
        _parameters.Add("@itemList", CustomTableDataHelper.CreateCustomQueryParameter(itemList));
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }
}