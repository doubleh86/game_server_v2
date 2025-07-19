using ApiServer.Services;
using Microsoft.AspNetCore.Mvc;
using ServerFramework.CommonUtils.DateTimeHelper;

namespace ApiServer.Controllers;

[ApiController]
[Route("[controller]")]
public class CheatController : ApiControllerBase
{
    public CheatController(ApiServerService service) : base(service)
    {
    }

    /// <summary>
    /// URL : /set-cheat-server?isEnable=true
    /// </summary>
    /// <param name="isEnable"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("set-cheat-server")]
    public async Task<ActionResult<string>> SetCheatServer(bool isEnable)
    {
        _service.SetCheatEnable(isEnable);
        return await Task.FromResult($"Cheat: [{_service.CheatEnable}]");
    }

    /// <summary>
    /// URL /change-server-time?changeTime=2025-07-20 17:00:00
    /// </summary>
    /// <param name="changeTime"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("change-server-time")]
    public async Task<ActionResult<string>> ChangeServerTime(DateTime changeTime)
    {
        
        var serverTime = TimeZoneHelper.CreateDateTimeToServerTime(changeTime);
        TimeZoneHelper.SetFakeDateTime(serverTime.ToUtcTime());
        return await Task.FromResult($"CurrentServerTime: [{TimeZoneHelper.ServerTimeNow.ToTimeString()}][{TimeZoneHelper.UtcNow.ToTimeString()}]");
    }
    
}