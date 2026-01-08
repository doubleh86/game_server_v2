using ClientTest.Models;
using ClientTest.Socket;
using NetworkProtocols;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.NotifyServerProtocols.TwoWayModels;

namespace ClientTest.Handlers;

public class NotifyServerHandler : TCPPacketHandler
{
    public NotifyServerHandler(TestSession client) : base(client)
    {
        _client = client;
    }

    protected override void _RegisterHandler()
    {
        base._RegisterHandler();
        _handler.Add((int)NotifyServerKeys.ResponseTest, OnTest);
    }
    
    private void OnTest(NetworkPackage package)
    {
        var receivedPackage = MemoryPackHelper.Deserialize<TestCommandResponse>(package.Body);
        if(receivedPackage == null)
            return;
        
        Console.WriteLine($"Received test request: {receivedPackage.TestText}");
        if (_client is not TestSession client)
            return;
        
        client.SendTestCommand();
    }

    protected override void OnConnected(NetworkPackage package)
    {
        base.OnConnected(package);

        if (_client is not TestSession client)
            return;
        
        client.SendTestCommand();
    }
}