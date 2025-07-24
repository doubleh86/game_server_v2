namespace ServerFramework.CommonUtils.DateTimeHelper;

public static class TimeZoneHelper
{
    private static string _defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss";
    private static TimeZoneInfo _timeZoneInfo;
    private static int _diffUtcHours;
    private static IDateTimeProvider _dateTimeProvider;
    
    public static TimeZoneInfo CurrentTimeZone => _timeZoneInfo ??= TimeZoneInfo.Local;
    public static int DiffUtcHours => _diffUtcHours;
    public static DateTime UtcNow => _dateTimeProvider.UtcNow;
    public static DateTime ServerTimeNow => _dateTimeProvider.UtcNow.ToServerTime();
     
    public static void Initialize(string timeZoneId, string defaultDateTimeVisible = "yyyy-MM-dd HH:mm:ss", IDateTimeProvider dateTimeProvider = null)
    {
        SetDateTimeProvider(dateTimeProvider);
        
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

    public static DateTime CreateDateTimeToServerTime(DateTime dateTime)
    {
        return CreateDateTimeToServerTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }
    public static DateTime CreateDateTimeToServerTime(int year, int month, int day, int hour, int min, int sec)
    {
        var newDateTime = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Unspecified);
        var toServerTime = TimeZoneInfo.ConvertTime(newDateTime, _timeZoneInfo);
        
        return toServerTime;
    }

    public static DateTime CreateDateTimeToServerTime(DateOnly dateOnly, int hour, int minute, int sec)
    {
        return CreateDateTimeToServerTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, hour, minute, sec);
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

    public static void SetDateTimeProvider(IDateTimeProvider dateTimeProvider)
    {
        dateTimeProvider ??= new DefaultDateTimeProvider();
        _dateTimeProvider = dateTimeProvider;
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

    public static string ToTimeString(this DateTime from, bool showTimeZone = true)
    {
        if (from.Kind == DateTimeKind.Unspecified)
        {
            if(showTimeZone == true)
                return from.ToString(_defaultDateTimeVisible) + $"({_timeZoneInfo.DisplayName})";
            return from.ToString(_defaultDateTimeVisible);
        }
        
        if(showTimeZone == true)
            return from.ToString(_defaultDateTimeVisible) + $"({from.Kind})";
        return from.ToString(_defaultDateTimeVisible);
    }

#endregion DateTime Extension Methods
}