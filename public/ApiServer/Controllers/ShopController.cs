using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using DbContext.Common;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Shop;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopController : ApiControllerBase
{
    private readonly EventService _eventService;
    public ShopController(ApiServerService service, EventService eventService) : base(service)
    {
        _eventService = eventService;
    }
    
    [HttpPost]
    [Route("shop-buy")]
    public async Task<ActionResult<string>> ShopBuyItem([FromBody] ShopBuyCommand.Request request)
    {
        var response = new ShopBuyCommand.Response();
        try
        {
            var (dbInfo, slaveDbInfo) = await _Initialize(request);
            using var handler = new ShopHandler(request.AccountId, _service, _eventService);
            await handler.InitializeModulesAsync(dbInfo, slaveDbInfo);

            var (inventoryInfo, assetInfo) = await handler.BuyShopItemAsync(request.ItemIndex, request.Amount);
            response.Item = inventoryInfo;
            response.Asset = assetInfo;

            return _OkResponse(GameResultCode.Ok, response, handler.RefreshDataHelper);
        }
        catch (ApiServerException e)
        {
            return _ErrorResponse(response, e.ResultCode, e.Message);
        }
        catch (DbContextException e)
        {
            return _ErrorResponse(response, GameResultCode.DbError, $"[{e.ResultCode}][{e.Message}]");
        }
        catch (Exception e)
        {
            return _ErrorResponse(response, GameResultCode.SystemError, e.Message);
        }
    }
}