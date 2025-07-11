using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(ApiServerService service) : ApiControllerBase(service)
{
    [HttpGet]
    [Route("test")]
    public async Task<ActionResult<string>> TestAsync()
    {
        _service.LoggerService.Information("test !!!!");
        await Task.Delay(10);
        
        return Ok("Test");
    }

    [HttpPost]
    [Route("get-account-info")]
    public async Task<ActionResult<string>> GetAccountInfoAsync([FromBody] AuthCommand.Request request)
    {
        var response = new AuthCommand.Response();
        try
        {
            using var handler = new AuthHandler(_service);
            var result = await handler.GetAccountInfoAsync(request.LoginId);
            
            var sessionHandler = _GetSessionHandler(result.AccountId);
            var sessionInfo = sessionHandler.CreateSessionInfo(request.LoginId, 
                                                               result.AccountId, 
                                                               request.Sequence, 
                                                               request.SubSequence, 
                                                               result.GetMainDbInfo(),
                                                               result.GetMainDbInfo(isSlave: true));
            
            if(sessionInfo == null)
                throw new ApiServerException(ResultCode.SystemError, "Create session failed");
            
            if(await sessionHandler.SetRedisSessionInfo() == false)
                throw new ApiServerException(ResultCode.SystemError, "Save session failed");
            
            response.AccountId = result.AccountId;
            response.AccountType = result.AccountType;
            response.Token = sessionInfo.AccessToken;

            return _OkResponse(ResultCode.Ok, response);
        }
        catch (ApiServerException e)
        {
            return _ErrorResponse(response, e.ResultCode, e.Message);
        }
        catch (Exception ex)
        {
            return _ErrorResponse(response, ResultCode.GameError, ex.Message);
        }
    }
}