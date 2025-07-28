using NetworkProtocols.WebApi.ToClientModels;

namespace NetworkProtocols.WebApi.Commands.Mail;

public class GetMailCommand
{
    public class Request : RequestBase
    {
        public Request() : base("/mail/get-mail-box")
        {
            
        }
    }

    public class Response : ResponseBase
    {
        public List<MailInfo> MailInfoList { get; set; }
        public List<MailInfo> EventMailInfoList { get; set; }
        public List<MailInfo> NotificationMailInfoList { get; set; }
    }
}