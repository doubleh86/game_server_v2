using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.MailCommands;

public class ReceiveMailRewardAsync : ProcBaseModelAsync<bool, bool>
{
    private const string _ProcedureName = "dbo.gsp_receive_mail_reward";

    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; init; }
        public List<MailInfoDbResult> MailList { get; init; }
    }
    
    public ReceiveMailRewardAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
        _parameters.Add("@itemList", CustomTableDataHelper.CreateCustomQueryParameter(inParams.MailList));
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }
}