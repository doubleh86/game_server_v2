using ClientTest.Handlers;
using ClientTest.Helpers;
using ClientTest.Socket;
using ClientTest.Socket.TCPClient;
using NetworkProtocols;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.NotifyServerProtocols.TwoWayModels;

namespace ClientTest.Models;

public class TestSession : ITCPClient, IDisposable
{
    private TCPNetworkWrapper _tcpNetwork;
    
    public void Connect(string ip, int port, TCPPacketHandler networkHandler)
    {
        _tcpNetwork = new TCPNetworkWrapper(ip, port, 0, networkHandler);
        _tcpNetwork.ConnectServer();
    }

    public void SendTestCommand()
    {
        var package = new TestCommandRequest();
        package.TestText = "테스트 패킷입니다. 테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다." +
                           "테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다." +
                           "테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다. 테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다." +
                           "테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다." +
                           "테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다." +
                           "테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.테스트 패킷입니다.";
        
        var sendBuffer = TCPNetworkHelper.MakePackage((int)NotifyServerKeys.RequestTest, MemoryPackHelper.Serialize(package));
        _tcpNetwork.GetTcpSession().Send(sendBuffer);
    }
    
    public void Dispose()
    {
        _tcpNetwork?.Disconnect();
    }
}