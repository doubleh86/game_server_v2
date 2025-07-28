
using ApiServer.GameService.GameModules;
using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using DbContext.Common;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Inventory;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class InventoryController : ApiControllerBase
{
    private readonly EventService _eventService;
    public InventoryController(ApiServerService service, EventService eventService) : base(service)
    {
        _eventService = eventService;
    }

    [HttpPost]
    [Route("use-item")]
    public async Task<ActionResult<string>> UseInventoryItem([FromBody] ItemUseCommand.Request request)
    {
        var response = new ItemUseCommand.Response();
        try
        {
            var (dbInfo, slaveDbInfo) = await _Initialize(request);
            using var handler = new InventoryHandler(request.AccountId, _service, _eventService);
            await handler.InitializeModulesAsync(dbInfo, slaveDbInfo);

            var inventoryItem = await handler.UseInventoryItemAsync(request.ItemIndex, request.Quantity);
            response.UseItemInfo = inventoryItem;

            return _OkResponse(ResultCode.Ok, response);
        }
        catch (ApiServerException e)
        {
            return _ErrorResponse(response, e.ResultCode, e.Message);
        }
        catch (DbContextException e)
        {
            return _ErrorResponse(response, ResultCode.DbError, $"[{e.ResultCode}][{e.Message}]");
        }
        catch (Exception e)
        {
            return _ErrorResponse(response, ResultCode.SystemError, e.Message);
        }
    }
}