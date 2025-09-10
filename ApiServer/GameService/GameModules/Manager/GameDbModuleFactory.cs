using ApiServer.GameService.Models;
using ApiServer.Utils;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules.Manager;

public static class GameDbModuleFactory
{
    private static readonly Dictionary<Type, Func<long, SqlServerDbInfo, SqlServerDbInfo, IGameModule>> _map = new()
    {
        { typeof(AssetInfoModule),    (accountId, master, slave) => new AssetInfoModule(accountId, master, slave) },
        { typeof(GameUserModule),     (accountId, master, slave) => new GameUserModule(accountId, master, slave) },
        { typeof(InventoryModule),    (accountId, master, slave) => new InventoryModule(accountId, master, slave) },
        { typeof(MailModule),         (accountId, master, slave) => new MailModule(accountId, master, slave) },
    };
    
    public static IGameModule CreateModule(Type moduleType, long accountId, SqlServerDbInfo master, SqlServerDbInfo slave)
    {
        if (_map.TryGetValue(moduleType, out var factory) == true)
            return factory(accountId, master, slave);

        throw new ApiServerException(GameResultCode.SystemError, $"Please register module factory [{moduleType.Name}]");
    }
    
}