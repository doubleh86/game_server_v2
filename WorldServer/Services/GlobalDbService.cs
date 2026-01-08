using System.Threading.Channels;
using DbContext.GameDbContext;
using ServerFramework.CommonUtils.Helper;

namespace WorldServer.Services;

public class GlobalDbService : IDisposable
{
    private int _shardCount;
    private Channel<Func<IGameDbContext, Task>>[] _dbShards;
    private IGameDbContext[] _dbContexts;
    
    private LoggerService _loggerService;
    

    public void Initialize(int shardCount, LoggerService loggerService)
    {
        _loggerService = loggerService;
        _shardCount = shardCount;
        _dbShards = new Channel<Func<IGameDbContext, Task>>[shardCount];
        _dbContexts = new IGameDbContext[shardCount];
        for (int i = 0; i < shardCount; i++)
        {
            _dbContexts[i] = MySqlGameDbContext.Create();
            _dbShards[i] = Channel.CreateUnbounded<Func<IGameDbContext, Task>>(new UnboundedChannelOptions()
            {
                SingleReader = true
            });

            var shardIndex = i;
            Task.Factory.StartNew(() => _ProcessJobsAsync(shardIndex), TaskCreationOptions.LongRunning);
        }
    }

    public void PushJob(long key, Func<IGameDbContext, Task> job)
    {
        int index = (int)(Math.Abs(key) % _shardCount);
        _dbShards[index].Writer.TryWrite(job);
    }

    private async Task _ProcessJobsAsync(int index)
    {
        var reader = _dbShards[index].Reader;
        var dbContext = _dbContexts[index];
        
        while (await reader.WaitToReadAsync())
        {
            while (reader.TryRead(out var job))
            {
                try
                {
                    await job(dbContext);
                }
                catch (Exception e)
                {
                    _loggerService.Warning($"Error processing job in shard {index}", e);
                }
            }
        }
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}