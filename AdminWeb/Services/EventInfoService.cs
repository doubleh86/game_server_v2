using System.Collections.Concurrent;
using DataTableLoader.Models.EventModels;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.Shared.Enums;
using ServerFramework.CommonUtils.EventHelper;
using SharedDbContext = DbContext.SharedContext.SqlServerContext.SharedDbContext;

namespace AdminWeb.Services;

public class EventInfoService : EventServiceBase<EventDbResult>, IDisposable
{
    private ConcurrentBag<EventDbResult> _registeredEvents;
    private AdminToolServerService _serverService;

    private readonly SharedDbContext _dbContext;

    public EventInfoService(AdminToolServerService serverService)
    {
        _registeredEvents = [];
        _serverService = serverService;
        _dbContext = SharedDbContext.Create();
    }

    public async Task InitializeAsync()
    {
        var result = await _dbContext.GetEventInfoListAsync();
        if (result == null)
            return;
        
        _registeredEvents.Clear();
        _registeredEvents = new ConcurrentBag<EventDbResult>(result);
    }

    public List<EventDbResult> GetRegisteredEvents()
    {
        return _registeredEvents.ToList();
    }

    public async Task CreateNewEventAsync(EventTableBase.EventCategory eventCategory, int eventTableIndex, EventPeriodType eventPeriodType, 
                                          DateTime startDate, DateTime endDate, DateTime expiryDate, 
                                          string openTime, string closeTime, List<DayOfWeek> openDays)
    {
        var newEvent = new EventDbResult
        {
            event_type_id = (int)eventCategory,
            event_period_type = (int)eventPeriodType,
            event_table_index = eventTableIndex,
            event_start_date = startDate,
            event_end_date = endDate,
            event_expiry_date = expiryDate
        };
        
        newEvent.SetEventExtraValue("", "", []);

        if (eventPeriodType != EventPeriodType.Default)
        {
            newEvent.SetEventExtraValue(openTime, closeTime, openDays);
        }
        
        if (await _dbContext.CreateEventInfoAsync(newEvent) == true)
        {
            var result = await _dbContext.GetEventInfoListAsync();
            _registeredEvents.Clear();
            _registeredEvents = new ConcurrentBag<EventDbResult>(result);
        }
    }

    public async Task<int> RemoveEventAsync(List<EventDbResult> events, bool isRemove)
    {
        var success = 0;
        foreach (var eventInfo in events)
        {
            if (await _dbContext.RemoveEventInfoAsync(eventInfo, isRemove) == false)
                continue;
            
            Interlocked.Increment(ref success);
        }

        if (success < 1) 
            return success;
        
        var result = await _dbContext.GetEventInfoListAsync();
        _registeredEvents.Clear();
        _registeredEvents = new ConcurrentBag<EventDbResult>(result);

        return success;
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}