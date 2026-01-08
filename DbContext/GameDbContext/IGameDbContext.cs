using DbContext.GameDbContext.DbResultModel;

namespace DbContext.GameDbContext;

public interface IGameDbContext : IDisposable
{
    Task<PlayerInfoResult> GetPlayerInfoAsync(long accountId);
}
