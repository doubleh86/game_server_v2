using System.Text.Json;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;

namespace ServerFramework.CommonUtils.EventHelper;

public abstract class EventBaseData
{
    private EventExtraValue _extraValue;
    public abstract (DateTime, DateTime) GetStartEndDateTimeUTC();
    public abstract EventExtraValue GetExtraValue();
    public abstract DateTime StartDateUtc { get; set; }
    public abstract DateTime EndDateUtc { get; set; }
    public abstract DateTime ExpireDateUtc { get; set; }
    protected void _SetEventExtraValue(string openTime, string closeTime, List<DayOfWeek> openDayOfWeekList, out string jsonString)
    {
        var extraValue = GetExtraValue();
        if(extraValue == null)
            extraValue = new EventExtraValue();
        
        extraValue.OpenTime = openTime;
        extraValue.CloseTime = closeTime;

        extraValue.OpenDayOfWeek = string.Join(',', openDayOfWeekList.Select(x => (int)x));
        jsonString = JsonSerializer.Serialize(extraValue);
    }

    protected EventExtraValue _GetExtraValue(string jsonString)
    {
        if (_extraValue != null)
            return _extraValue;

        if (string.IsNullOrEmpty(jsonString) == true)
            return null;

        try
        {
            _extraValue = JsonSerializer.Deserialize<EventExtraValue>(jsonString);
            return _extraValue;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

public class EventExtraValue
{
    public string OpenTime { get; set; }
    public string CloseTime { get; set; }
        
    public string OpenDayOfWeek { get; set; }
    private List<DayOfWeek> _openDayOfWeekList;

    public string GetLogString()
    {
        return $"[OpenTime: {OpenTime}][CloseTime: {CloseTime}][OpenDayOfWeek: {OpenDayOfWeek}]";
    }
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