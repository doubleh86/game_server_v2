using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace Scheduler.Services.gRPCService;

public class GrpcChannelPool
{
    private readonly ConcurrentDictionary<string, GrpcChannel> _channels = new();

    public GrpcChannel GetChannel(string address)
    {
        return _channels.GetOrAdd(address, _ => GrpcChannel.ForAddress(address)); 
    }
}