using DbContext.SharedContext.DbResultModel;

namespace DbContext.SharedContext;

public interface ISharedDbContext : IDisposable
{
    Task<GetAccountDbResult> GetAccountInfoAsync(string loginId);
    Task<int> CreateAccountAsync(string loginId);
    Task<List<EventDbResult>> GetEventInfoListAsync();

}