namespace ServerFramework.CommonUtils.Helper;

public static class TimeZoneHelper
{
    private static string _defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss";
    private static TimeZoneInfo _timeZoneInfo;
    private static int _diffUtcHours;
    private static IDateTimeProvider _dateTimeProvider;
    
    public static TimeZoneInfo CurrentTimeZone => _timeZoneInfo ??= TimeZoneInfo.Local;
    public static int DiffUtcHours => _diffUtcHours;
    public static DateTime UtcNow => _dateTimeProvider.UtcNow;
     
    public static void Initialize(string timeZoneId, string defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss", IDateTimeProvider dateTimeProvider = null)
    {
        _dateTimeProvider = dateTimeProvider ?? new DefaultDateTimeProvider();
        
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _diffUtcHours = (int)_timeZoneInfo.GetUtcOffset(UtcNow).TotalHours;
        
        _defaultDateTimeVisible = defaultDateTimeVisible;
    }
    
    public static DateTime GetNextDateTimeDayOfWeek(DateTime from, DayOfWeek dayOfWeek)
    {
        var daysToAdd = ((int)dayOfWeek - (int)from.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) 
            daysToAdd = 7; // 오늘이면 다음 주로 넘김
        
        return from.Date.AddDays(daysToAdd);
    }

    public static DateTime CreateDateTimeToServerTime(int year, int month, int day, int hour, int min, int sec)
    {
        var newDateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Unspecified);
        var toServerTime = TimeZoneInfo.ConvertTime(newDateTime, _timeZoneInfo);
        
        return toServerTime;
    }

    public static DateTime CreateDateTimeToUtc(int year, int month, int day, int hour, int min, int sec)
    {
        var newDateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);
        return newDateTime;
    }

    public static void SetFakeDateTime(DateTime fixedTime)
    {
        if (_dateTimeProvider is not FakeDateTimeProvider fakeDateTimeProvider)
            return;
        
        fakeDateTimeProvider.SetUtcNow(fixedTime);
    }
    
    
#region DateTime Extension Methods

    // UTC Time => TimeZone time
    public static DateTime ToServerTime(this DateTime from)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(from, _timeZoneInfo);    
    }
     

    // UTC 가 들어오면?
    public static DateTime ToUtcTime(this DateTime from)
    {
        return TimeZoneInfo.ConvertTimeToUtc(from, _timeZoneInfo);
    }

    public static string ToServerTimeString(this DateTime from)
    {
        return from.ToServerTime().ToString(_defaultDateTimeVisible) + $"({_timeZoneInfo.DisplayName})";
    }

#endregion DateTime Extension Methods
}