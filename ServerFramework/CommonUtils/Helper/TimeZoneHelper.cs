namespace ServerFramework.CommonUtils.Helper;

public static class TimeZoneHelper
{
    public static string _defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss";
    private static TimeZoneInfo _timeZoneInfo;
    private static int _diffUtcHours;
    
    public static TimeZoneInfo CurrentTimeZone => _timeZoneInfo ??= TimeZoneInfo.Local;
    public static int DiffUtcHours => _diffUtcHours;
     
    public static void Initialize(string timeZoneId, string defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss")
    {
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _diffUtcHours = (int)_timeZoneInfo.GetUtcOffset(DateTime.UtcNow).TotalHours;
        
        _defaultDateTimeVisible = defaultDateTimeVisible;
    }

    // UTC Time => TimeZone time
    public static DateTime ToServerTime(this DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _timeZoneInfo);    
    }

    public static string ToServerTimeString(this DateTime dateTime)
    {
        return dateTime.ToServerTime().ToString(_defaultDateTimeVisible) + $"({_timeZoneInfo.DisplayName})";
    }
}