// See https://aka.ms/new-console-template for more information

using ClientTest.Handlers;
using ClientTest.Models;

Console.WriteLine("Run Test Client");
Console.WriteLine("1: TCP Session");
Console.WriteLine("2: World Session");
var command = Console.ReadLine();

switch (command)
{
    case "1":
    {
        var tcpClient = new TestSession();
        tcpClient.Connect("127.0.0.1", 18080, new NotifyServerHandler(tcpClient));
        return;
    }
    case "2":
    {
        for (int i = 0; i < 1; i++)
        {
            var tcpClient = new TestSession();
            tcpClient.Connect("127.0.0.1", 28080, new WorldServerHandler(tcpClient));    
        }
        return;
    }
}