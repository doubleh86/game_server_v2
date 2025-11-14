using DbContext.Common.Models;
using NetworkProtocols.Shared.Enums;
using NetworkProtocols.WebApi.ToClientModels;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.EventHelper;
using ServerFramework.CommonUtils.RDBUtils;
using ServerFramework.SqlServerServices.CommandModel;

namespace DbContext.SharedContext.DbResultModel;

public class EventDbResult : EventBaseData, IDbInParameters, IHasClientModel<GameEventInfo>
{
    public long event_id { get; set; }
    public int event_type_id { get; set; }
    public int event_period_type { get; set; } // EventPeriodType enum
    public int event_table_index { get; set; }
    public string event_extra_value { get; set; }
    public DateTime event_start_date { get; set; }
    public DateTime event_end_date { get; set; }
    public DateTime event_expiry_date { get; set; }
    
    public override DateTime StartDateUtc {
        get => event_start_date;
        set => event_start_date = value;
    }

    public override DateTime EndDateUtc
    {
        get => event_end_date;
        set => event_end_date = value;
    }

    public override DateTime ExpireDateUtc
    {
        get => event_expiry_date;
        set => event_expiry_date = value;
    }

    public void SetEventExtraValue(string openTime, string closeTime, List<DayOfWeek> openDayOfWeekList)
    {
        _SetEventExtraValue(openTime, closeTime, openDayOfWeekList, out var jsonString);
        event_extra_value = jsonString;
    }
    
    public override EventExtraValue GetExtraValue()
    {
        return _GetExtraValue(event_extra_value);
    }

    public override (DateTime, DateTime) GetStartEndDateTimeUTC()
    {
        if (event_period_type == (int)EventPeriodType.Default)
        {
            return (StartDateUtc,  EndDateUtc);
        }

        var extraValue = GetExtraValue();
        if(extraValue == null)
            return (DateTime.MinValue, DateTime.MinValue);

        DateTime openTimeUtc;
        DateTime closeTimeUtc;
        switch ((EventPeriodType)event_period_type)
        {
            case EventPeriodType.WeekDay:
                (openTimeUtc, closeTimeUtc) = _GetOpenCloseWeekDayTypeUTC(extraValue);
                break;
            case EventPeriodType.TimeRange:
                (openTimeUtc, closeTimeUtc) = _GetOpenCloseTimeRangeTypeUTC(extraValue);
                break;
            case EventPeriodType.WeekDayAndTimeRange:
                (openTimeUtc, closeTimeUtc) = _GetOpenCloseWeekDayAndTimeRangeTypeUTC(extraValue);
                break;
            default:
                return (DateTime.MinValue, DateTime.MinValue);
        }

        _GetRealStartEndDateTimeUTC(ref openTimeUtc, ref closeTimeUtc);
        return (openTimeUtc, closeTimeUtc);
    }

    private void _GetRealStartEndDateTimeUTC(ref DateTime openTimeUtc, ref DateTime closeTimeUtc)
    {
        if (openTimeUtc < StartDateUtc)
        {
            openTimeUtc = StartDateUtc;
        }

        if (closeTimeUtc > EndDateUtc)
        {
            closeTimeUtc = EndDateUtc;
        }
    }
    
    // 특정 시간 대 오픈
    // OpenTime, CloseTime 사용
    // 서버의 TimeZone 정보 값을 입력한다.
    private (DateTime, DateTime) _GetOpenCloseTimeRangeTypeUTC(EventExtraValue extraValue)
    {
        var serverTimeNow = TimeZoneHelper.ServerTimeNow;
        if(serverTimeNow < event_start_date.ToServerTime())
            serverTimeNow = event_start_date.ToServerTime();
        
        var (todayOpenDate, todayCloseDate) = extraValue.GetOpenCloseServerTime(serverTimeNow);
        if (todayCloseDate < serverTimeNow)
        {
            var openDate = todayOpenDate.AddDays(1);
            var closeDate = todayCloseDate.AddDays(1);
            return (openDate.ToUtcTime(), closeDate.ToUtcTime());    
        }
        
        return (todayOpenDate.ToUtcTime(), todayCloseDate.ToUtcTime());
    }

    // ex) 2025.07.14 00:00:00 ~ 2025.07.14 23:59:59
    // EventPeriodType의 WeekDay
    private (DateTime, DateTime) _GetOpenCloseWeekDayTypeUTC(EventExtraValue extraValue)
    {
        var serverTimeNow = TimeZoneHelper.ServerTimeNow;
        if(serverTimeNow < event_start_date.ToServerTime())
            serverTimeNow = event_start_date.ToServerTime();
        
        var todayDayOfWeek = serverTimeNow.DayOfWeek;
        
        var openDayOfWeeks =  extraValue.OpenDayOfWeekList();
        if (openDayOfWeeks.Contains(todayDayOfWeek) == true)
        {
            var openTime = serverTimeNow.Date;
            var closeTime = serverTimeNow.Date.AddDays(1).AddSeconds(-1);
            
            return (openTime.ToUtcTime(), closeTime.ToUtcTime());
        }
        
        var nextOpenDays = openDayOfWeeks.Select(dayOfWeek => TimeZoneHelper.GetNextDateTimeDayOfWeek(serverTimeNow, dayOfWeek)).ToList().Min();
        var nextOpenTime = nextOpenDays.Date;
        var nextCloseTime = nextOpenTime.Date.AddDays(1).AddSeconds(-1);

        return (nextOpenTime.ToUtcTime(), nextCloseTime.ToUtcTime());
    }

    private (DateTime, DateTime) _GetOpenCloseWeekDayAndTimeRangeTypeUTC(EventExtraValue extraValue)
    {
        var serverTimeNow = TimeZoneHelper.ServerTimeNow;
        if(serverTimeNow < event_start_date.ToServerTime())
            serverTimeNow = event_start_date.ToServerTime();
        
        var todayDayOfWeek = serverTimeNow.DayOfWeek;
        
        var openDayOfWeeks =  extraValue.OpenDayOfWeekList();
        if (openDayOfWeeks.Contains(todayDayOfWeek) == true)
        {
            var (todayOpenDate, todayCloseDate) = extraValue.GetOpenCloseServerTime(serverTimeNow); 
            if(serverTimeNow < todayCloseDate)
                return (todayOpenDate.ToUtcTime(), todayCloseDate.ToUtcTime());
        }
     
        var nextOpenDays = openDayOfWeeks.Select(dayOfWeek => TimeZoneHelper.GetNextDateTimeDayOfWeek(serverTimeNow, dayOfWeek)).ToList().Min();
        var (openTime, closeTime) = extraValue.GetOpenCloseServerTime(nextOpenDays);
        
        return (openTime.ToUtcTime(), closeTime.ToUtcTime());
    }

    public GameEventInfo ToClient(EventStatus status)
    {
        var toClient = ToClient();
        toClient.EventStatus = status;
        
        return toClient;
    }

    public GameEventInfo ToClient()
    {
        var (startDateUtc, endDateUtc) = GetStartEndDateTimeUTC();
        
        return new GameEventInfo()
        {
            EventId = event_id,
            EventIndex = event_table_index,
            EventTypeId = event_type_id,
            
            StartDateTime = startDateUtc,
            EndDateTime = endDateUtc,
            ExpireDateTime = ExpireDateUtc
        };
    }
}