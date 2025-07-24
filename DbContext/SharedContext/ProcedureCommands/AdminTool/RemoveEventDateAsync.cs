using DbContext.Common;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands.AdminTool;

public class RemoveEventDateAsync : ProcBaseModelAsync<bool, bool>
{
    public struct InParameters : IDbInParameters
    {
        public long EventId { get; init; }
        public bool IsRemove { get; init; }
    }
    
    private const string _ProcedureName = "dbo.gsp_remove_event_data";
    
    public RemoveEventDateAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@eventId", inParams.EventId);
        _parameters.Add("isRemove", inParams.IsRemove == true ? 1 : 0);
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }

}