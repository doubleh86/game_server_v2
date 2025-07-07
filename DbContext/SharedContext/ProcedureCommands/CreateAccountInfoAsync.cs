using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.SharedContext.ProcedureCommands;

public class CreateAccountInfoAsync : ProcBaseModelAsync<int, int>
{
    private new const string _ProcedureName = "dbo.gsp_create_account_info";
    public CreateAccountInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) : base(dbContext, _ProcedureName, transaction)
    {
    }

    public void SetParameters(string loginId)
    {
        _parameters.Add("@loginId", loginId);
    }

    public override async Task<int> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode();
    }
}