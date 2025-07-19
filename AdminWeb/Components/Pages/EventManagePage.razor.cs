using AdminWeb.Components.Pages.Modals;
using BlazorBootstrap;
using DbContext.SharedContext.DbResultModel;
using Microsoft.AspNetCore.Components;

namespace AdminWeb.Components.Pages;

public partial class EventManagePage : ComponentBase
{
    [Inject] private ModalService _modalService { get; set; }
    private CreateGameEventModal _modal;
    private Grid<EventDbResult> _grid;

    private async Task _OnClickButton()
    {
        await _modal.ShowModalAsync();
    }

    private async Task OnCreateResult(bool result)
    {
        await _grid.RefreshDataAsync();
    }
}