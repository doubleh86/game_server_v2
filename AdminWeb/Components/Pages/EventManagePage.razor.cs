using AdminWeb.Components.Pages.Modals;
using BlazorBootstrap;
using DataTableLoader.Models.EventModels;
using DbContext.SharedContext.DbResultModel;
using Microsoft.AspNetCore.Components;
using NetworkProtocols.Shared.Enums;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;

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

    private string _GetNearByOpenTime(EventDbResult dbResult)
    {
        if (dbResult.ExpireDateUtc < TimeZoneHelper.UtcNow)
            return "-";
        
        var (startDateTime, endDateTime) = dbResult.GetStartEndDateTimeUTC();
        return $"{startDateTime.ToServerTime()} ~ {endDateTime.ToServerTime()}";
    }

    private string _GetOpenDaysToString(EventDbResult dbResult)
    {
        if (dbResult.event_period_type == (int)EventPeriodType.Default || dbResult.event_period_type == (int)EventPeriodType.TimeRange)
            return "-";

        var extraValue = dbResult.GetExtraValue();
        return string.Join(",", extraValue.OpenDayOfWeekList());
    }

    private string _GetOpenTimeToString(EventDbResult dbResult)
    {
        if (dbResult.event_period_type == (int)EventPeriodType.Default || dbResult.event_period_type == (int)EventPeriodType.WeekDay)
            return "-";
            
        var extraValue = dbResult.GetExtraValue();
        var (openHour, openMinute, openSeconds) = CommonHelper.ParseStringTimeToInt(extraValue.OpenTime);
        var (closeHour, closeMinute, closeSeconds) = CommonHelper.ParseStringTimeToInt(extraValue.CloseTime);
        return $"{openHour:D2}:{openMinute:D2}:{openSeconds:D2} ~ {closeHour:D2}:{closeMinute:D2}:{closeSeconds:D2}";
    }

    private string _GetEventTypeToString(EventDbResult dbResult)
    {
        return ((EventTableBase.EventCategory)dbResult.event_type_id).ToString();
    }

    private string _GetEventPeriodTypeToString(EventDbResult dbResult)
    {
        return  ((EventPeriodType)dbResult.event_period_type).ToString();
    }
}