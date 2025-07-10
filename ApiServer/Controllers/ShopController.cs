using ApiServer.Handlers;
using ApiServer.Services;
using ApiServer.Utils;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Shop;
using NetworkProtocols.WebApi.Commands.User;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopController(ApiServerService service) : ApiControllerBase(service)
{
    [HttpPost]
    [Route("shop-buy")]
    public async Task<ActionResult<string>> ShopBuyItem([FromBody] ShopBuyCommand.Request request)
    {
        var response = new ShopBuyCommand.Response();
        try
        {
            var (dbInfo, slaveDbInfo) = await _Initialize(request);
            using var handler = new ShopHandler(request.AccountId, _service);
            await handler.InitializeModulesAsync(dbInfo, slaveDbInfo);
            
            response.Items = await handler.BuyShopItemAsync(request.ItemIndex);
            
            return _OkResponse(ResultCode.Ok, response);
        }
        catch (ApiServerException e)
        {
            return _ErrorResponse(response, e.ResultCode, e.Message);
        }
        catch (Exception e)
        {
            return _ErrorResponse(response, ResultCode.SystemError, e.Message);
        }
    }
}