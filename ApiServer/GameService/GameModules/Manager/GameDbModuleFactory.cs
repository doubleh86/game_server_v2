using ApiServer.GameService.Models;
using ApiServer.Utils;
using NetworkProtocols.WebApi;
using ServerFramework.SqlServerServices.Models;

namespace ApiServer.GameService.GameModules.Manager;

public static class GameDbModuleFactory
{
    public static IGameModule CreateModule(string name, long accountId, SqlServerDbInfo master, SqlServerDbInfo slave)
    {
        return name switch
        {
            nameof(AssetInfoModule) => new AssetInfoModule(accountId, master, slave),
            nameof(GameUserModule) => new GameUserModule(accountId, master, slave),
            nameof(InventoryModule) => new InventoryModule(accountId, master, slave),
            nameof(MailModule) => new MailModule(accountId, master, slave),
            _ => throw new ApiServerException(GameResultCode.SystemError, $"Please add module factory [{name}]")
        };
    }
}