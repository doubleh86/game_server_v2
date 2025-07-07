using ServerFramework.CommonUtils.Helper;
using SuperSocket.Connection;
using SuperSocket.Server;

namespace NotifyServer.Services.NetworkService;

public class UserSessionInfo : AppSession
{
    private long _identifier;
    public long Identifier => _identifier;
    private NotifyServerService _serverService => Server as NotifyServerService;

    public void SetIdentifier(long identifier)
    {
        _identifier = identifier;
    }
    
    protected override ValueTask OnSessionConnectedAsync()
    {
        _serverService.GetLoggerService().Information("Connected to NotifyServer");
        _serverService.GetUserService().AddUser(this);
        
        return ValueTask.CompletedTask;
    }

    protected override ValueTask OnSessionClosedAsync(CloseEventArgs e)
    {
        Console.WriteLine($"Closed from server [{e.Reason}]");
        _serverService.GetUserService().RemoveUser(_identifier);
        
        return ValueTask.CompletedTask;
    }
}