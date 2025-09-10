using NetworkProtocols.WebApi;
using NetworkProtocols.WebApi.Commands.Auth;
using NetworkProtocols.WebApi.Commands.Mail;
using NetworkProtocols.WebApi.ToClientModels;

namespace ApiServerTest.ApiTest;

public class MailTest : WebSendTestBase
{
    private List<MailInfo> _mailList = null;
    
    [Test, Order(0)]
    public async Task GetMailTest()
    {
        _loginInfo.Sequence += 1;
        var request = new GetMailCommand.Request()
        {
            AccountId = _loginInfo.AccountId,
            Sequence = _loginInfo.Sequence,
            SubSequence = _loginInfo.SubSequence,
            Token = _loginInfo.Token,
        };
        
        var response = await ApiTestHelper.SendPacket<GetMailCommand.Request, GetMailCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
        }

        _mailList = response.MailInfoList;
    }

    [Test, Order(1)]
    public async Task OpenMailTest()
    {
        if (_mailList == null || _mailList.Count == 0)
        {
            Console.WriteLine($"Mail is empty");
            return;
        }
            
        _loginInfo.Sequence += 1;
        var receivedMail = _mailList.FindAll(x => x.IsReceivedReward == false).Select(x => x.MailUid).ToList();
        var request = new OpenMailCommand.Request()
        {
            AccountId = _loginInfo.AccountId,
            Sequence = _loginInfo.Sequence,
            SubSequence = _loginInfo.SubSequence,
            Token = _loginInfo.Token,
            
            MailUidList = receivedMail
        };
        
        var response = await ApiTestHelper.SendPacket<OpenMailCommand.Request, OpenMailCommand.Response>(request, LoginInfo.ServerUrl);
        if (response.ResultCode != (int)GameResultCode.Ok)
        {
            Console.WriteLine($"Error Message : {response.DebugMessage}");
        }
    }
}