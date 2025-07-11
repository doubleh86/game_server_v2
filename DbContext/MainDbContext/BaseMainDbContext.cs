using ServerFramework.SqlServerServices.DapperUtils;
using ServerFramework.SqlServerServices.Models;

namespace DbContext.MainDbContext;

public abstract class BaseMainDbContext : DapperServiceBase
{
    protected BaseMainDbContext(SqlServerDbInfo serverInfo) : base(serverInfo)
    {
    }
}