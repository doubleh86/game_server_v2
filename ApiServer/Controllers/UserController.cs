using ApiServer.Handlers;
using ApiServer.Services;
using ApiServer.Utils;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.User;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(ApiServerService service) : ApiControllerBase(service)
{
    [HttpPost]
    [Route("get-user-info")]
    public async Task<ActionResult<string>> GetUserInfo([FromBody] GetUserCommand.Request request)
    {
        var response = new GetUserCommand.Response();
        try
        {
            var dbInfo = await _Initialize(request);
            var handler = new UserHandler(_service, dbInfo);
            var result = await handler.GetUserInfoAsync(request.AccountId);
            response.UserLevel = result.user_level;
            
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