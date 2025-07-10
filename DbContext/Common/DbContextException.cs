using NetworkProtocols.WebApi;

namespace DbContext.Common;

public class DbContextException : Exception
{
    public override string Message { get; }
    public readonly DbContextResultCode ResultCode;
    public DbContextException(DbContextResultCode resultCode, string message)
    {
        Message = message;
        ResultCode = resultCode;
    }
}