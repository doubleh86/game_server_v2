namespace NetworkProtocols.WebApi.Commands.Mail;

public class OpenMailCommand
{
    public class Request : RequestBase
    {
        public List<long> MailUidList  { get; set; }
        
        public Request() : base("/mail/open-mail")
        {
            
        }
    }

    public class Response : RefreshResponse
    {
        public List<long> ReceiveFailUidList { get; set; }
    }
}