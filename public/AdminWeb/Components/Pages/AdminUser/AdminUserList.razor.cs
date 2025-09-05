using AdminWeb.Components.Pages.Modals;
using AdminWeb.Services.Utils;
using BlazorBootstrap;
using DbContext.AdminDbContext;
using DbContext.AdminDbContext.DbResultModel;
using Microsoft.AspNetCore.Components;

namespace AdminWeb.Components.Pages.AdminUser;

public partial class AdminUserList : ComponentBase
{
    [Inject] private ToastService _toastService { get; set; }
    [Inject] private ModalService _modalService { get; set; }
    
    private List<AdminUserDbModel> _adminUserList;
    private CreateAdminUserModal _modal;
    private Grid<AdminUserDbModel> _grid;
    private async Task<GridDataProviderResult<AdminUserDbModel>> AdminUserListProvider(GridDataProviderRequest<AdminUserDbModel> request)
    {
        if (_adminUserList is null)
            _adminUserList = await _GetAdminUserList();
        
        return await Task.FromResult(request.ApplyTo(_adminUserList));
    }
    private async Task<List<AdminUserDbModel>> _GetAdminUserList()
    {
        using var dbContext = AdminDbContext.Create();
        var result = await dbContext.GetAllAdminUserList();

        return result ?? [];
    }

    private async Task _OnClickButton()
    {
        await _modal.ShowModalAsync();
    }

    private async Task OnCreateResult(bool result)
    {
        if (result == true)
        {
            _adminUserList = await _GetAdminUserList();
            await _grid.RefreshDataAsync();
        }
    }

    private async Task _OnClickButton2()
    {
        _toastService.Notify(ToastMessageCreator.CreateToastWithoutTitle(ToastType.Info, "Click"));
    }
}