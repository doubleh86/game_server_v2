using ServerFramework.CommonUtils.DateTimeHelper;
using ServerFramework.CommonUtils.Helper;

namespace ApiServerTest.SystemTest;

public class CommonHelperTest
{
    private ConfigurationHelper _configurationHelper;
    [SetUp]
    public void Setup()
    {
        
        var configFiles = new List<string> {"appsettings.json", "Settings/sqlSettings.json"};
        
        _configurationHelper = new ConfigurationHelper();
        _configurationHelper.Initialize(configFiles);
        var serviceTimeZone = _configurationHelper.GetValue("ServiceTimeZone", "UTC");
        
        TimeZoneHelper.Initialize(serviceTimeZone, dateTimeProvider: new FakeDateTimeProvider(DateTime.UtcNow));
    }

    [Test, Order(0)]
    public void CheckOverlappedDateTest()
    {
        var startDate1 = new DateTime(2025, 7, 24, 0, 0, 0);
        var endDate1 = new DateTime(2025, 7, 30, 0, 0, 0);
        var startDate2 = new DateTime(2025, 7, 28, 0, 0, 0);
        var endDate2 = new DateTime(2025, 8, 10, 0, 0, 0);
        
        // true
        bool overlapped = CommonHelper.CheckOverlappedDate(startDate1, endDate1, startDate2, endDate2);
        Console.WriteLine("Overlapped dates are: " + overlapped);
        
        // false
        startDate2 = new DateTime(2025, 7, 31, 0, 0, 0);
        overlapped = CommonHelper.CheckOverlappedDate(startDate1, endDate1, startDate2, endDate2);
        Console.WriteLine("Overlapped dates are: " + overlapped);
    }
}