using NetworkProtocols.WebApi;

namespace ApiServer.Utils;

public class ApiServerException : Exception
{
    public override string Message { get; }
    public readonly ResultCode ResultCode;

    public ApiServerException(ResultCode resultCode, string message)
    {
        Message = message;
        ResultCode = resultCode;
    }
}