using DbContext.Common;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.Data.SqlClient;
using NetworkProtocols.WebApi;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands.GameUserCommands;

public class GetGameUserInfoAsync : ProcBaseModelAsync<GameUserDbModel, GameUserDbModel>
{
    public struct InParameters : IDbInParameters
    {
        public long AccountId { get; set; }
    }
    
    private const string _ProcedureName = "dbo.gsp_get_game_user";
    public GetGameUserInfoAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public override void SetParameters(IDbInParameters inParameters)
    {
        if (inParameters is not InParameters inParams)
            throw new DbContextException(DbErrorCode.InParameterWrongType, "GetGameUserInfoAsync Type is wrong");
        
        _parameters.Add("@accountId", inParams.AccountId);
    }
    
    public override async Task<GameUserDbModel> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }

}