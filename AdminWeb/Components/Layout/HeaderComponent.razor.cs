using System.Collections.Generic;
using System.Threading.Tasks;
using AdminWeb.Components.Pages.Offcanvas;
using BlazorBootstrap;
using DbContext.SharedContext.DbResultModel;
using Microsoft.AspNetCore.Components;

namespace AdminWeb.Components.Layout;

public partial class HeaderComponent : ComponentBase
{
    private async Task<List<GetAccountDbResult>> _GetAccountInfoListAsync(FilterItem filter)
    {
        if(filter == null || string.IsNullOrEmpty(filter.Value) == true)
            return [];
        
        return await CachedService.GetAccountListAsync(filter.Value);
    }
    
    private async Task _ShowUserInfo()
    {
        if (_isSelectedUser == false)
            return;

        var userDbInfo = UserInfoService.GetUserDbModel();
        if (userDbInfo.account_id < 1)
            return;
        
        var parameters = new Dictionary<string, object> { { "UserDbInfo", userDbInfo } };
        await _offCanvas.ShowAsync<ShowUserInfoOffCanvas>(title: "User Info", parameters: parameters);
    }
}