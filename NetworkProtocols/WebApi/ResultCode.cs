namespace NetworkProtocols.WebApi;

public enum ResultCode
{
    Ok = 0,
    GameError = 1,
    SystemError = 2,
}

public enum DbContextResultCode
{
    NotRegisteredFactory = 1
}