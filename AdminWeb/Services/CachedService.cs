using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;

namespace AdminWeb.Services;

public class CachedService
{
    private DateTime _lastReloadTime;
    private List<GetAccountDbResult> _accountList = [];

    public async Task InitializeAsync()
    {
        await LoadAccountTotalListAsync();
    }

    public async Task<List<GetAccountDbResult>> GetAccountListAsync(string filter)
    {
        await LoadAccountTotalListAsync();
        return _accountList.FindAll(x => x.LoginId.ToLower().Contains(filter.ToLower())).Take(30).ToList();
    }

    public async Task<bool> CheckAccountId(string loginId)
    {
        await LoadAccountTotalListAsync();
        return _accountList.FirstOrDefault(x=> x.LoginId.ToLower() == loginId.ToLower()) != null;
    }

    public async Task LoadAccountTotalListAsync(bool forceReload = false)
    {
        if (forceReload == true)
        {
            using var dbContext = SharedDbContext.Create();
            _accountList = await dbContext.GetAccountInfoTotalListAsync();
            
            _lastReloadTime = DateTime.UtcNow;
            return;
        }
        
        var timeDiff = DateTime.UtcNow - _lastReloadTime;
        if (timeDiff.TotalSeconds > (60 * 5))
        {
            using var dbContext = SharedDbContext.Create();
            _accountList = await dbContext.GetAccountInfoTotalListAsync();
            
            _lastReloadTime = DateTime.UtcNow;
        }
    }
}