using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.MailCommands;

public class InsertMailItemInfoAsync : ProcBaseModelAsync<bool, bool>
{
    private const string _ProcedureName = "dbo.gsp_insert_mail_info";

    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; init; }
        public List<MailInfoDbResult> InsertMailList { get; init; }
    }
    
    public InsertMailItemInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) 
        : base(dbContext, _ProcedureName, transaction)
    {
    }

    public override void SetParameters(IDbInParameters inParameters)
    {
        if(inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, $"[{GetType().Name}] Parameter Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
        _parameters.Add("@mailList", CustomTableDataHelper.CreateCustomQueryParameter(inParams.InsertMailList));
    }

    public override async Task<bool> ExecuteProcedureAsync()
    {
        await _RunDbProcedureReturnDynamicAsync();
        _CheckExceptionError();

        return _GetResultCode() == 0;
    }
}