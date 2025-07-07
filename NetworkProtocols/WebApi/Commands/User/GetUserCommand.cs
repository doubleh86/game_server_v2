namespace NetworkProtocols.WebApi.Commands.User;

public class GetUserCommand
{
    public class Request : RequestBase
    {
        public Request() : base("/user/get-user-info")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public int UserLevel { get; set; }
        
    }
}