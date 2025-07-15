using DbContext.Common;
using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class CreateEventDataAsync : ProcBaseModelAsync<int, int>
{
    private const string _ProcedureName = "dbo.gsp_update_event_data";
    
    public CreateEventDataAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not EventDbResult inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@eventId", inParams.event_id);
        _parameters.Add("@eventTypeId", inParams.event_type_id);
        _parameters.Add("@tableIndex", inParams.event_table_index);
        _parameters.Add("@startDate", inParams.event_start_date);
        _parameters.Add("@endDate", inParams.event_end_date);
        _parameters.Add("@expiryDate", inParams.event_expiry_date);
        _parameters.Add("@extraValue", inParams.event_extra_value);
        _parameters.Add("@eventPeriodType", inParams.event_period_type);
    }

    public override async Task<int> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode();
    }

}