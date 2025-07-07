using DbContext.MainDbContext.DbResultModel;
using Microsoft.Data.SqlClient;
using ServerFramework.SqlServerServices.CommandModel;
using ServerFramework.SqlServerServices.DapperUtils;

namespace DbContext.MainDbContext.ProcedureCommands;

public class CreateNewGameUserAsync : ProcBaseModelAsync<GameUserDbModel, GameUserDbModel>
{
    private const string _ProcedureName = "dbo.gsp_create_new_game_user";
    
    public CreateNewGameUserAsync(DapperServiceBase dbContext, SqlTransaction transaction = null) : base(dbContext, _ProcedureName, transaction)
    {
    }
    
    public void SetParameters(long accountId)
    {
        _parameters.Add("@accountId", accountId);
    }
    
    public override async Task<GameUserDbModel> ExecuteProcedureAsync()
    {
        var result = await _RunDbProcedureReturnModelAsync();
        _CheckExceptionError();
        
        return result.FirstOrDefault();
    }
    
}