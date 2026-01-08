namespace NetworkProtocols.Socket.NotifyServerProtocols;

public enum NotifyServerKeys : int
{
    RequestStart = 1000,
    RequestTest = 1001,
    RequestEnd,
    
    ResponseStart = 2000,
    ResponseTest = 2001,
    ResponseEnd,
}

public enum WorldServerKeys : int
{
    RequestStart = 2000,
    RequestWorldJoin = 2001,
    GameCommandRequest = 2002,
    RequestEnd,
    
    ResponseStart = 3000,
    ResponseWorldJoin = 3001,
    GameCommandResponse = 3002,
    ResponseEnd,
}