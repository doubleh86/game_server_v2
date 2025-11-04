using MissionCreator.SubThreads;

namespace MissionCreator.Main;

public class MainThread
{
    private ScheduleThread _scheduleThread;

    public void Start()
    {
        try
        {
            _scheduleThread = new ScheduleThread();
            _scheduleThread.Start();
            while (true)
            {
                Console.WriteLine("Main thread running");
                Thread.Sleep(50);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
    
}