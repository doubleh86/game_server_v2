using NetworkProtocols.WebApi;

namespace ApiServer.Utils;

public class ApiServerException : Exception
{
    public override string Message { get; }
    public readonly GameResultCode ResultCode;

    public ApiServerException(GameResultCode resultCode, string message)
    {
        Message = message;
        ResultCode = resultCode;
    }
}