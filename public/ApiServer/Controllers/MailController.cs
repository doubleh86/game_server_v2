using ApiServer.GameService.Handlers.GameHandlers;
using ApiServer.Services;
using ApiServer.Utils;
using DbContext.Common;
using Microsoft.AspNetCore.Mvc;
using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Mail;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class MailController : ApiControllerBase
{
    public MailController(ApiServerService service) : base(service)
    {
    }

    [HttpPost]
    [Route("get-mail")]
    public async Task<ActionResult<string>> GetMailBoxInfo([FromBody] GetMailCommand.Request request)
    {
        var response = new GetMailCommand.Response();
        try
        {
            var (dbInfo, slaveDbInfo) = await _Initialize(request);
            using var handler = new MailHandler(request.AccountId, _service);
            await handler.InitializeModulesAsync(dbInfo, slaveDbInfo);

            var mailDbResults = await handler.GetMailListAsync();
            response.MailInfoList = mailDbResults.Select(x => x.ToClient()).ToList();

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