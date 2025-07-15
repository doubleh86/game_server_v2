using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.User;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ApiControllerBase
{
    private readonly EventService _eventService;
    public UserController(ApiServerService service, EventService eventService) : base(service)
    {
        _eventService = eventService;
    }
    
    [HttpPost]
    [Route("get-user-info")]
    public async Task<ActionResult<string>> GetUserInfo([FromBody] GetUserCommand.Request request)
    {
        var response = new GetUserCommand.Response();
        try
        {
            var (dbInfo, slaveDbInfo) = await _Initialize(request);
            using var handler = new UserHandler(request.AccountId, _service);
            await handler.InitializeModulesAsync(dbInfo, slaveDbInfo);
            
            var (gameUserInfo, inventoryInfo, assetInfo) = await handler.GetUserInfoAsync();
            response.UserInfo = gameUserInfo;
            response.Assets = assetInfo;
            response.InventoryItems = inventoryInfo;

            response.GameEventData = _eventService.GetEvents(48);
            
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