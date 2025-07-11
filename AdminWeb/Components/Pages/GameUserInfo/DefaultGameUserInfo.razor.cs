using AdminWeb.Services.GameUserService;
using AdminWeb.Services.GameUserService.Modules;
using BlazorBootstrap;
using DbContext.MainDbContext.DbResultModel.AdminTool;
using DbContext.MainDbContext.DbResultModel.GameDbModels;
using Microsoft.AspNetCore.Components;

namespace AdminWeb.Components.Pages.GameUserInfo;

public partial class DefaultGameUserInfo : ComponentBase
{
    [Inject] private ToastService _toastService { get; set; }
    [Inject] private ModalService _modalService { get; set; }
    
    private Grid<InventoryDbResult> _grid;
    private Grid<AssetDbResult> _assetGrid;
    private AdminInventoryModule _inventoryModule;
    private AdminAssetModule _assetModule;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender == true)
        {
            if (await GameUserInfoService.IsSelectedUser() == true)
            {
                var dbInfo = GameUserInfoService.GetAccountDbResult;
                _inventoryModule = new AdminInventoryModule(dbInfo);
                _assetModule = new AdminAssetModule(dbInfo);
            }
        }

        await _grid.RefreshDataAsync();
        await _assetGrid.RefreshDataAsync();
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task<GridDataProviderResult<AssetDbResult>> AssetInfoListProviderAsync(GridDataProviderRequest<AssetDbResult> request)
    {
        if(_assetModule == null)
            return await Task.FromResult(new GridDataProviderResult<AssetDbResult>() {Data = new List<AssetDbResult>(), TotalCount = 0});

        var list = await _assetModule.GetAssetListAsync();
        return await Task.FromResult(request.ApplyTo(list));
    }
    
    private async Task<GridDataProviderResult<InventoryDbResult>> InventoryListProvider(GridDataProviderRequest<InventoryDbResult> request)
    {
        if (_inventoryModule == null)
            return await Task.FromResult(new GridDataProviderResult<InventoryDbResult>() {Data = new List<InventoryDbResult>(), TotalCount = 0}); 
        
        var list = await _inventoryModule.GetInventoryListAsync();
        return await Task.FromResult(request.ApplyTo(list));
    }
}