using Scheduler.Services;

namespace Scheduler.SubThreads;

public abstract class SubThreadBase(ScheduleService scheduleService)
{
    protected ScheduleService _scheduleService = scheduleService;
}