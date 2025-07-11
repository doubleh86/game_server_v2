using DbContext.MainDbContext.DbResultModel.GameDbModels;
using ServerFramework.CommonUtils.Helper;

namespace DbContext.MainDbContext.DbResultModel.AdminTool;

public class AdminGameUserDbModel : GameUserDbModel
{
    private TimeZoneInfo _timeZoneInfo;

    private string _visibleTime;
    // ForAdmin
    public string LoginId;
    public string CreateDate;
    public string UpdateDate => update_date.ToServerTime().ToString(_visibleTime);
    

    public static AdminGameUserDbModel ToAdminModel(GameUserDbModel baseDbModel, TimeZoneInfo timeZoneInfo, string visibleTime = "yyyy-MM-dd HH:mm:ss")
    {
        var result = new AdminGameUserDbModel
        {
            account_id = baseDbModel.account_id,
            user_level = baseDbModel.user_level,
            user_exp = baseDbModel.user_exp,
            update_date = baseDbModel.update_date,
            _timeZoneInfo = timeZoneInfo,
            _visibleTime = visibleTime
        };

        return result;
    }
}