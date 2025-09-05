using DbContext.Common;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.GameUserCommands;

public class ChangeExpAndLevelAsync : ProcBaseModelAsync<bool, bool>
{
    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; init; }
        public int Level { get; init; }
        public int Exp { get; init; }
    }
    
    private const string _ProcedureName = "dbo.gsp_change_exp_and_level";
    
    public ChangeExpAndLevelAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
        _parameters.Add("@level", inParams.Level);
        _parameters.Add("@exp", inParams.Exp);
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }

    
}