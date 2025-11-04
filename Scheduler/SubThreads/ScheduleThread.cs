namespace MissionCreator.SubThreads;

public class ScheduleThread
{
    private Thread _thread;

    public void Start()
    {
        _thread = new Thread(StartThread);
        _thread.Start();
    }

    private void StartThread()
    {
        while (true)
        {
            Console.WriteLine("ScheduleThread Start");
            Thread.Sleep(50);
        }
    }
}