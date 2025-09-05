using DbContext.Common.Models;
using NetworkProtocols.WebApi.ToClientModels;

namespace DbContext.MainDbContext.DbResultModel.GameDbModels;

public class GameUserDbModel : IHasClientModel<GameUserInfo>
{
    public long account_id { get; set; }
    public int user_level { get; set; }
    public int user_exp { get; set; }
    public DateTime update_date { get; set; }


    public void ChangeExpAndLevel(int exp, int level)
    {
        user_exp = exp;
        user_level = level;
    }
    public GameUserInfo ToClient()
    {
        return new GameUserInfo
        {
            UserLevel = user_level,
            UserExperience = user_exp,
        };
    }
}