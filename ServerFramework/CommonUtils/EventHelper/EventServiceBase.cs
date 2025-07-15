using ServerFramework.CommonUtils.DateTimeHelper;

namespace ServerFramework.CommonUtils.EventHelper;

public class EventServiceBase<T> where T : EventBaseData
{
    private List<T> _registeredEventList;
    public void Initialize(List<T> eventList)
    {
        _registeredEventList = eventList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>(진행중, 오픈예정, 만료예정)</returns>
    protected (List<T>, List<T>, List<T>) _GetEventList(int upcomingHours)
    {
        var processingEvent = _GetProcessingEvents();
        var upcomingEvent = _GetUpcomingEvents(upcomingHours);
        var upcomingExpiredEvent = _GetUpcomingExpiredEvents();
        
        return (processingEvent, upcomingEvent, upcomingExpiredEvent);
    }

    private List<T> _GetUpcomingExpiredEvents()
    {
        var currentUtc = TimeZoneHelper.UtcNow;
        var result = new List<T>();

        foreach (var eventInfo in _registeredEventList)
        {
            // 만료 이벤트 제외
            if (eventInfo.ExpireDateUtc < currentUtc)
                continue;
            
            var (startDateUtc, endDateUtc) = eventInfo.GetStartEndDateTimeUTC();
            
            // 미오픈 제외
            if (startDateUtc > endDateUtc)
                continue;

            // 진행 중 제외
            if (endDateUtc > currentUtc)
                continue;
            
            result.Add(eventInfo);
        }

        return result;
    }

    private List<T> _GetProcessingEvents()
    {
        var currentUtc = TimeZoneHelper.UtcNow;
        var result = new List<T>();

        foreach (var eventInfo in _registeredEventList)
        {
            // 만료 이벤트 제외
            if (eventInfo.ExpireDateUtc < currentUtc)
                continue;

            var (startDateUtc, endDateUtc) = eventInfo.GetStartEndDateTimeUTC(); 
            
            // 미오픈
            if(startDateUtc > currentUtc)
                continue;

            if (endDateUtc < currentUtc)
                continue;
            
            result.Add(eventInfo);
        }

        return result;
    }

    private List<T> _GetUpcomingEvents(int hour = 1)
    {
        var currentUtc = TimeZoneHelper.UtcNow;
        var result = new List<T>();
        foreach (var eventInfo in _registeredEventList)
        {
            // 만료 이벤트 제외
            if (eventInfo.ExpireDateUtc < currentUtc)
                continue;

            var (startDateUtc, endDateUtc) = eventInfo.GetStartEndDateTimeUTC(); 
            // 종료 이벤트 제외
            if (endDateUtc < currentUtc)
                continue;
            
            // 진행 중 이벤트는 제외
            if (startDateUtc < currentUtc && currentUtc < endDateUtc)
                continue;
            
            var diff = startDateUtc - currentUtc;
            if (diff.TotalHours > hour)
                continue;
            
            result.Add(eventInfo);
        }

        return result;
    }
}