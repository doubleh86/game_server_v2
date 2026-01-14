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
