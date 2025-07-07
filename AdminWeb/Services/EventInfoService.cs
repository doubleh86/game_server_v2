using System.Collections.Concurrent;
using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;

namespace AdminWeb.Services;

public class EventInfoService : IDisposable
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

    public async Task CreateNewEventAsync(int eventType, int eventTableIndex, DateTime startDate, DateTime endDate, DateTime expiryDate)
    {
        var newEvent = new EventDbResult
        {
            event_type = eventType,
            event_table_index = eventTableIndex,
            event_start_date = startDate,
            event_end_date = endDate,
            event_expiry_date = expiryDate
        };
        
        if (await _dbContext.CreateEventInfoAsync(newEvent) == true)
        {
            var result = await _dbContext.GetEventInfoListAsync();
            _registeredEvents.Clear();
            _registeredEvents = new ConcurrentBag<EventDbResult>(result);
        }
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}