using System.Net;
using System.Net.Sockets;

namespace ClientTest.Socket.TCPClient;

public class TCPConnector : IDisposable
{
    public delegate void ConnectHandler(TCPSession session);
        
    private System.Net.Sockets.Socket _socket;
    private readonly ManualResetEventSlim _connectEvent = new ManualResetEventSlim(false);

    public ConnectHandler ConnectionCompleteHandler;
        
    public async Task<bool> Connect(string ip, int port, int timeout, ulong accountId)
    {
        if (false == IPAddress.TryParse(ip, out var address))
        {
            try
            {
                address = (await Dns.GetHostEntryAsync(ip)).AddressList[0];
            }
            catch
            {
                return false;
            }
        }

        _socket = new System.Net.Sockets.Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        var tcpSession = new TCPSession(_socket);
        tcpSession.AccountId = accountId;

        _socket.BeginConnect(ip, port, ConnectComplete, tcpSession);
        _connectEvent.Wait();

        return true;
    }

    private void ConnectComplete(IAsyncResult ar)
    {
        _connectEvent.Set();
            
        var tcpSession = ar.AsyncState as TCPSession;
        if (tcpSession == null)
        {
            return;
        }
 
        try
        {
            tcpSession.GetSocket().EndConnect(ar);
            if (tcpSession.GetSocket().Connected == false)
            {
                throw new Exception("Socket is not connected");
            }
                
            if (ConnectionCompleteHandler != null)
            {
                ConnectionCompleteHandler(tcpSession);
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine($"[ConnectComplete][{e.ErrorCode}][{e.Message}]");
            tcpSession.Disconnect(SessionCloseReason.ServerShutdown);
            tcpSession.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ConnectComplete][{e.Message}]");
            tcpSession.Disconnect(SessionCloseReason.Unknown);
            tcpSession.Dispose();
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _connectEvent?.Dispose();
    }
}