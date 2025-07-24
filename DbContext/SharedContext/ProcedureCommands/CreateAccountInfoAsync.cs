using DbContext.Common;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands;

public class CreateAccountInfoAsync : ProcBaseModelAsync<int, int>
{
    public struct InParameters : IDbInParameters
    {
        public string LoginId { get; init; }
    }
    private const string _ProcedureName = "dbo.gsp_create_account_info";
    public CreateAccountInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        _parameters.Add("@loginId", inParams.LoginId);
    }

    public override async Task<int> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode();
    }

}