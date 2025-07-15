using ServerFramework.CommonUtils.DateTimeHelper;

namespace ApiServer.Services;

public partial class ApiServerService
{
    private bool _cheatEnable = false;
    public bool CheatEnable => _cheatEnable;

    public void SetCheatEnable(bool enable)
    {
        _loggerService.Information($"Set Cheat : [{enable}]");
        _cheatEnable = enable;
        
        _ChangeDateTimeProvider();
    }

    private void _ChangeDateTimeProvider()
    {
        TimeZoneHelper.SetDateTimeProvider(_cheatEnable == true ? new FakeDateTimeProvider(DateTime.UtcNow) : null);
    }
}