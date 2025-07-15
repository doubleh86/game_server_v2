using System.Text.Json;
using NetworkProtocols.Shared.Enums;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.CommandModel;

namespace DbContext.SharedContext.DbResultModel;

public class EventDbResult : IDbInParameters
{
    internal class EventExtraValue
    {
        public string OpenTime { get; set; }
        public string CloseTime { get; set; }
        
        public string OpenDayOfWeek { get; set; }
        private List<DayOfWeek> _openDayOfWeekList;

        public List<DayOfWeek> OpenDayOfWeekList()
        {
            if(_openDayOfWeekList != null)
                return _openDayOfWeekList;
            
            if (string.IsNullOrEmpty(OpenDayOfWeek) == true)
                return [];
            
            _openDayOfWeekList = OpenDayOfWeek.Split(',').Select(Enum.Parse<DayOfWeek>).ToList();
            return _openDayOfWeekList;
        }

        public (DateTime, DateTime) GetOpenCloseServerTime(DateTime toServerTime)
        {
            return (GetOpenServerTime(toServerTime), GetCloseServerTime(toServerTime));
        }

        public DateTime GetOpenServerTime(DateTime toServerTime)
        {
            var (hour, minute, seconds) = CommonHelper.ParseStringTimeToInt(OpenTime);
            return TimeZoneHelper.CreateDateTimeToServerTime(toServerTime.Year, toServerTime.Month, toServerTime.Day, 
                                                             hour, minute, seconds);
        }

        public DateTime GetCloseServerTime(DateTime toServerTime)
        {
            
            var (hour, minute, seconds) = CommonHelper.ParseStringTimeToInt(CloseTime);
            return TimeZoneHelper.CreateDateTimeToServerTime(toServerTime.Year, toServerTime.Month, toServerTime.Day, 
                                                             hour, minute, seconds);
        }
    } 
    public long event_id { get; set; }
    public int event_type_id { get; set; }
    public int event_period_type { get; set; } // EventPeriodType enum
    public int event_table_index { get; set; }
    public string event_extra_value { get; set; }
    public DateTime event_start_date { get; set; }
    public DateTime event_end_date { get; set; }
    public DateTime event_expiry_date { get; set; }

    private EventExtraValue _extraValue = null;

    public void SetEventExtraValue(string openTime, string closeTime, List<DayOfWeek> openDayOfWeekList)
    {
        var extraValue = _GetExtraValue();
        if(extraValue == null)
            extraValue = new EventExtraValue();
        
        extraValue.OpenTime = openTime;
        extraValue.CloseTime = closeTime;

        extraValue.OpenDayOfWeek = string.Join(',', openDayOfWeekList.Select(x => (int)x));
        event_extra_value = JsonSerializer.Serialize(extraValue);
    }
    
    private EventExtraValue _GetExtraValue()
    {
        if (_extraValue != null)
            return _extraValue;

        if (string.IsNullOrEmpty(event_extra_value) == true)
            return null;

        try
        {
            _extraValue = JsonSerializer.Deserialize<EventExtraValue>(event_extra_value);
            return _extraValue;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public (DateTime, DateTime) GetOpenCloseDateTimeUTC(out EventStatus status)
    {
        status = EventStatus.Expired;
        if (event_period_type == (int)EventPeriodType.Default)
        {
            status = EventStatus.Active;
            return (event_start_date,  event_end_date);
        }

        var extraValue = _GetExtraValue();
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

        return (openTimeUtc, closeTimeUtc);
    }

    // 특정 시간 대 오픈
    // OpenTime, CloseTime 사용
    // 서버의 TimeZone 정보 값을 입력한다.
    private (DateTime, DateTime) _GetOpenCloseTimeRangeTypeUTC(EventExtraValue extraValue)
    {
        var serverTimeNow = TimeZoneHelper.UtcNow.ToServerTime();
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
        var serverTimeNow = TimeZoneHelper.UtcNow.ToServerTime();
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

        return (nextCloseTime.ToUtcTime(), nextCloseTime.ToUtcTime());
    }

    private (DateTime, DateTime) _GetOpenCloseWeekDayAndTimeRangeTypeUTC(EventExtraValue extraValue)
    {
        var serverTimeNow = TimeZoneHelper.UtcNow.ToServerTime();
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
}