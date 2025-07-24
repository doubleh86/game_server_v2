using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.Shared.Enums;
using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;
using ServerFramework.SqlServerServices.Models;

namespace ApiServerTest.SystemTest;

public class EventSystemTest
{
    private ConfigurationHelper _configurationHelper;
    private List<EventDbResult> _testEventDbResults;
    
    [SetUp]
    public void Setup()
    {
        var configFiles = new List<string> {"appsettings.json", "Settings/sqlSettings.json"};
        
        _configurationHelper = new ConfigurationHelper();
        _configurationHelper.Initialize(configFiles);
        _InitializeSqlServerDbInfo();
        var serviceTimeZone = _configurationHelper.GetValue("ServiceTimeZone", "UTC");
        TimeZoneHelper.Initialize(serviceTimeZone, dateTimeProvider: new FakeDateTimeProvider(new DateTime(2025, 7, 14, 2, 10, 0)));
        
        // _SetTestEventDbResults();
        _SetTestEventDbResultsV2();
    }

    private EventDbResult _CreateEventDbResult(int typeId, int tableIndex, DateTime startDate, DateTime endDate, 
                                               EventPeriodType periodType, List<DayOfWeek> weekDays, string openTime,
                                               string closeTime)
    {
        var defaultEvent = new EventDbResult
        {
            event_type_id = typeId,
            event_period_type = (int)periodType,
            event_table_index = tableIndex,
            event_start_date = startDate,
            event_end_date = endDate,
            event_expiry_date = endDate.AddDays(2),
            event_extra_value = "",
        };
        
        defaultEvent.SetEventExtraValue(openTime, closeTime, weekDays);

        return defaultEvent;
    }

    private void _SetTestEventDbResultsV2()
    {
        _testEventDbResults = [];
        var startDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 14, 0, 0);
        var endDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 14, 0, 0);
        var weekDayEvent = _CreateEventDbResult(2, 1, startDate.ToUtcTime(), endDate.ToUtcTime(), EventPeriodType.WeekDay,
                                                [DayOfWeek.Tuesday, DayOfWeek.Friday], "", "");
        _testEventDbResults.Add(weekDayEvent);
        
        startDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 15, 0, 0);
        endDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 15, 15, 0);
        var timeRange = _CreateEventDbResult(2, 2, startDate.ToUtcTime(), endDate.ToUtcTime(), EventPeriodType.TimeRange,
                                                [], "1400", "1800");
        _testEventDbResults.Add(timeRange);
        
        startDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 10, 0, 0);
        endDate = TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 15, 15, 0);
        var weekDayAndTimeRange = _CreateEventDbResult(2, 3, startDate.ToUtcTime(), endDate.ToUtcTime(), 
                                                       EventPeriodType.WeekDayAndTimeRange,
                                             [DayOfWeek.Tuesday, DayOfWeek.Friday], "1400", "1800");
        _testEventDbResults.Add(weekDayAndTimeRange);
    }
    
    
    private void _InitializeSqlServerDbInfo()
    {
        var sqlSettings = _configurationHelper.GetSection<SqlServerDbSettings>(nameof(SqlServerDbSettings));
        foreach (var (key, value) in sqlSettings.ConnectionInfos)
        {
            switch (key)
            {
                case nameof(SharedDbContext):
                    SharedDbContext.SetDefaultServerInfo(value);
                    break;
            }
        }
    }

    [Test, Order(0)]
    public async Task InsertEventTest()
    {
        using var dbContext = SharedDbContext.Create();
        var result = await dbContext.GetEventInfoListAsync();
        try
        {
            foreach (var item in _testEventDbResults)
            {
                if(result?.FirstOrDefault(x => x.event_table_index == item.event_table_index) == null)
                    await dbContext.CreateEventInfoAsync(item);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Test, Order(1)]
    public void CheckEventCloseOpenDateTimeTest()
    {
        using var dbContext = SharedDbContext.Create();
        var result = dbContext.GetEventInfoListAsync().GetAwaiter().GetResult();
        
        var testDates = new List<DateTime>
        {
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 15, 0, 0),
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 19, 30, 0), // Start 시간 테스트
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 13, 0, 0),
            
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 13, 23, 10, 1), // 7.13 00:00:00 ~ 7.13 23:59:59
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 2, 10, 0), // 7.14 00:00:00 ~ 7.14 23:59:59
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 11, 10, 2), // 7.14 00:00:00 ~ 7.14 23:59:59
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 23, 10, 2), // 7.14 00:00:00 ~ 7.14 23:59:59
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 1, 10, 2), // next 7.20 00:00:00 ~ 7.20 23:59:59
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 16, 16, 10, 2), // 7.16 16:00:00 ~ 7.16 18:00:00
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 16, 20, 10, 2), // 7.17 16:00:00 ~ 7.16 18:00:00
            // TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 20, 10, 2), // 7.23 16:00:00 ~ 7.16 18:00:00
        };

        foreach (var item in result)
        {
            if (item.event_period_type == (int)EventPeriodType.Default)
                continue;
            
            Console.WriteLine("===================================================================================================");
            Console.WriteLine($"EventType             : {(EventPeriodType)item.event_period_type}");
            Console.WriteLine($"EventInfo             : {item.GetExtraValue()?.GetLogString()}");
            

            foreach (var testDate in testDates)
            {
                TimeZoneHelper.SetFakeDateTime(testDate.ToUtcTime());
                var (openTimeUTC, closeTimeUTC) = item.GetStartEndDateTimeUTC();
                
                PrintLog(true, testDate, openTimeUTC, closeTimeUTC, item.StartDateUtc, item.EndDateUtc);
            }
            Console.WriteLine("===================================================================================================");
        }    
    }

    [Test, Order(2)]
    public void GetEventInfoListTest()
    {
        using var dbContext = SharedDbContext.Create();
        var result = dbContext.GetEventInfoListAsync().GetAwaiter().GetResult();

        foreach (var item in result)
        {
            var (startDateTime, endDateTime) = item.GetStartEndDateTimeUTC();
            Console.WriteLine($"StartDate:{startDateTime.ToServerTime()} EndDate:{endDateTime.ToServerTime()}");    
        }
        
    }
    private void PrintLog(bool isTimeZone, DateTime testDate, DateTime openTime, DateTime closeTime, DateTime startDate, DateTime endDate)
    {
        Console.WriteLine("---------------------------------------------------------------------------------------------------");
        if (isTimeZone == true)
        {
            Console.WriteLine($"Server Time[TimeZone] : {testDate}");
            Console.WriteLine($"ProcessTime[TimeZone] : {openTime.ToServerTime()} ~ {closeTime.ToServerTime()}");
            Console.WriteLine($"Period[TimeZone]      : {startDate.ToServerTime()} ~ {endDate.ToServerTime()}");
        }
        else
        {
            Console.WriteLine($"Server Time[UTC]      : {testDate.ToUtcTime()}");
            Console.WriteLine($"ProcessTime[UTC]      : {openTime} ~ {closeTime}");
            Console.WriteLine($"Period[UTC]           : {startDate} ~ {endDate}");
        }
        Console.WriteLine("---------------------------------------------------------------------------------------------------");
    }
    
}