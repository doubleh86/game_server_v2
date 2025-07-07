// See https://aka.ms/new-console-template for more information

using ClientTest.Handlers;
using ClientTest.Models;

Console.WriteLine("Run Test Client");
Console.WriteLine("1: TCP Session");
Console.WriteLine("2: RPC Session");
var command = Console.ReadLine();

switch (command)
{
    case "1":
    {
        var tcpClient = new TestSession();
        tcpClient.Connect("127.0.0.1", 18080, new NotifyServerHandler(tcpClient));
        return;
    }
}