using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class CreateEventDataAsync : ProcBaseModelAsync<int, int>
{
    private const string _ProcedureName = "dbo.update_event_data";
    
    public CreateEventDataAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(EventDbResult dbResult)
    {
        _parameters.Add("@eventId", dbResult.event_id);
        _parameters.Add("@eventType", dbResult.event_type);
        _parameters.Add("@tableIndex", dbResult.event_table_index);
        _parameters.Add("@startDate", dbResult.event_start_date);
        _parameters.Add("@endDate", dbResult.event_end_date);
        _parameters.Add("@expiryDate", dbResult.event_expiry_date);
    }

    public override async Task<int> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode();
    }
}