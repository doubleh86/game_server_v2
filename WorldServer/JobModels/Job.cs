namespace WorldServer.JobModels;

public abstract class Job(byte[] commandData)
{
    protected byte[] commandData { get; set; } = commandData;
    public abstract void Execute();
    public abstract ValueTask ExecuteAsync();
}