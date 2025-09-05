using System.Runtime.CompilerServices;
using NetworkProtocols.WebApi;

namespace DbContext.Common;

public class DbContextException : Exception
{
    public override string Message { get; }
    public readonly DbErrorCode ResultCode;
    public DbContextException(DbErrorCode resultCode, string message)
    {
        Message = message;
        ResultCode = resultCode;
    }
}