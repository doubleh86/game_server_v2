using DbContext.SharedContext;
using DbContext.SharedContext.DbResultModel;
using NetworkProtocols.Shared.Enums;
using ServerFramework.CommonUtils;
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
        
        _SetTestEventDbResults();
    }

    private void _SetTestEventDbResults()
    {
        _testEventDbResults = [];
        var currentDate = TimeZoneHelper.UtcNow;
        var defaultEvent = new EventDbResult
        {
            event_type_id = 1,
            event_period_type = (int)EventPeriodType.Default,
            event_table_index = 1,
            event_start_date = currentDate,
            event_end_date = currentDate.AddDays(7),
            event_expiry_date = currentDate.AddDays(10),
            event_extra_value = "",
        };

        _testEventDbResults.Add(defaultEvent);
        var weekDayEvent = new EventDbResult()
        {
            event_type_id = 1,
            event_period_type = (int)EventPeriodType.WeekDay,
            event_table_index = 2,
            event_start_date = currentDate,
            event_end_date = currentDate.AddDays(7),
            event_expiry_date = currentDate.AddDays(10),
        };
        weekDayEvent.SetEventExtraValue("", "", [DayOfWeek.Sunday, DayOfWeek.Monday]);

        var timeRangeEvent = new EventDbResult()
        {
            event_type_id = 1,
            event_period_type = (int)EventPeriodType.TimeRange,
            event_table_index = 3,
            event_start_date = currentDate,
            event_end_date = currentDate.AddDays(7),
            event_expiry_date = currentDate.AddDays(10),
        };

        timeRangeEvent.SetEventExtraValue("1400", "1600", []);
        _testEventDbResults.Add(timeRangeEvent);
        
        var weekDayAndTimeRangeEvent = new EventDbResult()
        {
            event_type_id = 1,
            event_period_type = (int)EventPeriodType.WeekDayAndTimeRange,
            event_table_index = 4,
            event_start_date = currentDate,
            event_end_date = currentDate.AddDays(7),
            event_expiry_date = currentDate.AddDays(10),
        };

        weekDayAndTimeRangeEvent.SetEventExtraValue("1600", "1800", [DayOfWeek.Wednesday, DayOfWeek.Thursday]);
        _testEventDbResults.Add(weekDayAndTimeRangeEvent);
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
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 13, 23, 10, 1), // 7.13 00:00:00 ~ 7.13 23:59:59
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 2, 10, 0), // 7.14 00:00:00 ~ 7.14 23:59:59
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 11, 10, 2), // 7.14 00:00:00 ~ 7.14 23:59:59
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 14, 23, 10, 2), // 7.14 00:00:00 ~ 7.14 23:59:59
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 15, 1, 10, 2), // next 7.20 00:00:00 ~ 7.20 23:59:59
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 16, 16, 10, 2), // 7.16 16:00:00 ~ 7.16 18:00:00
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 16, 20, 10, 2), // 7.17 16:00:00 ~ 7.16 18:00:00
            TimeZoneHelper.CreateDateTimeToServerTime(2025, 7, 18, 20, 10, 2), // 7.23 16:00:00 ~ 7.16 18:00:00
        };

        foreach (var item in result)
        {
            if (item.event_period_type == (int)EventPeriodType.Default)
                continue;

            foreach (var testDate in testDates)
            {
                TimeZoneHelper.SetFakeDateTime(testDate.ToUtcTime());
                var (openTimeUTC, closeTimeUTC) = item.GetOpenCloseDateTimeUTC(out _);
        
                Console.WriteLine($"=====[{(EventPeriodType)item.event_period_type}]=============================================================================");
                Console.WriteLine($"Test Time [UTC: {testDate.ToUtcTime()}][TimeZone : {testDate}]");
                Console.WriteLine($"OpenTime [UTC: {openTimeUTC}][TimeZone : {openTimeUTC.ToServerTime()}]");
                Console.WriteLine($"CloseTime [UTC: {closeTimeUTC}][TimeZone : {closeTimeUTC.ToServerTime()}]");
                Console.WriteLine("===================================================================================================");    
            }
        }    
    }
}