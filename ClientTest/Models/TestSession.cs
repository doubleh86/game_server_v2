using ClientTest.Handlers;
using ClientTest.Helpers;
using ClientTest.Socket;
using ClientTest.Socket.TCPClient;
using NetworkProtocols;
using NetworkProtocols.Socket;
using NetworkProtocols.Socket.NotifyServerProtocols;
using NetworkProtocols.Socket.NotifyServerProtocols.TwoWayModels;
using NetworkProtocols.Socket.WorldServerProtocols;

namespace ClientTest.Models;

public class TestSession : ITCPClient, IDisposable
{
    private TCPNetworkWrapper _tcpNetwork;
    
    public void Connect(string ip, int port, TCPPacketHandler networkHandler)
    {
        _tcpNetwork = new TCPNetworkWrapper(ip, port, 0, networkHandler);
        _tcpNetwork.ConnectServer();
    }
    
    public void SendGameCommand(GameCommandRequest command)
    {
        var sendBuffer = TCPNetworkHelper.MakePackage((int)WorldServerKeys.GameCommandRequest, MemoryPackHelper.Serialize(command));
        _tcpNetwork.GetTcpSession().Send(sendBuffer);
    }

    public void SendWorldJoinCommand()
    {
        var package = new WorldJoinCommandRequest
        {
            Identifier = 100
        };

        var sendBuffer = TCPNetworkHelper.MakePackage((int)WorldServerKeys.RequestWorldJoin, MemoryPackHelper.Serialize(package));
        _tcpNetwork.GetTcpSession().Send(sendBuffer);
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