using DbContext.SharedContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.SqlServerContext.ProcedureCommands.AdminTool;

public class GetEventInfoTotalList : ProcBaseModelAsync<List<EventDbResult>, EventDbResult>
{
    private const string _ProcedureName = "dbo.gsp_get_event_data_list";
    public GetEventInfoTotalList(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        // No Parameters
    }

    public override async Task<List<EventDbResult>> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.ToList();
    }

    
}