
using MissionCreator.Services;

namespace MissionCreator.SubThreads;

public abstract class SubThreadBase(ScheduleService scheduleService)
{
    protected ScheduleService _scheduleService = scheduleService;
}