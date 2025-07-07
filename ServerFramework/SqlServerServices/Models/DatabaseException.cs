using System.Runtime.CompilerServices;

namespace ServerFramework.SqlServerServices.Models;

public class DatabaseException : Exception
{
    public override string Message { get; }
    public ServerError ResultCode;
    
    public DatabaseException(ServerError resultCode, string message, [CallerLineNumber] int lineNumber = 0,
                             [CallerMemberName] string method = null)
    {
        Message = $"[ResultCode: {resultCode}][{message}]||[Line: {lineNumber}]|[Method: {method}]";
        ResultCode = resultCode;
    }
}
