namespace NetworkProtocols.WebApi.Commands.Auth;

public class AuthCommand
{
    public class Request : RequestBase
    {
        public string LoginId { get; set; }

        public Request() : base("/auth/get-account-info")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public long AccountId { get; set; }
        public int AccountType { get; set; }
        public string Token { get; set; }
    }
}