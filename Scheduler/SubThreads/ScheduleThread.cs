using Scheduler.Services;

namespace Scheduler.SubThreads;

public class ScheduleThread(ScheduleService scheduleService) : SubThreadBase(scheduleService)
{
    private Thread _thread;
    private CancellationTokenSource _cancelToken;

    public void Start()
    {
        _cancelToken = new CancellationTokenSource();
        _thread = new Thread(() => _StartThread(_cancelToken.Token))
        {
            IsBackground = true
        };
        
        _thread.Start();
        
    }

    private void _StartThread(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            Thread.Sleep(50);
        }
    }
}