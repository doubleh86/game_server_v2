using System.Data;
using DataTableLoader.Utils;
using DataTableLoader.Utils.Helper;
using Scheduler.Services;
using Scheduler.SubThreads;
using ServerFramework.CommonUtils.DateTimeHelper;

namespace Scheduler.Main;

public class ScheduleMain
{
    private ScheduleThread _scheduleThread;
    private readonly ScheduleService _scheduleService = new();
    
    public void Start()
    {
        Console.WriteLine("Start");
        try
        {
            _scheduleThread = new ScheduleThread(_scheduleService);
            _scheduleThread.Start();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public void Initialize()
    {
        _scheduleService.Initialize();
        
        var serviceTimeZone = _scheduleService.CustomConfiguration.GetValue("ServiceTimeZone", "UTC");
        TimeZoneHelper.Initialize(serviceTimeZone);
        
        var sqlInfo = _scheduleService.GetSqlServerDbInfo(nameof(DataTableDbService));
        DataHelper.Initialize(sqlInfo, _scheduleService.LoggerService);
        DataHelper.ReloadTableData();
    }
    
    public int GetGrpcPort()
    {
        return _scheduleService.CustomConfiguration.GetValue("GrpcPort", 5001);
    }
}