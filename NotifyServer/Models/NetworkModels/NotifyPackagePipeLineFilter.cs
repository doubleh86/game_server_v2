using System.Buffers;
using SuperSocket.ProtoBase;

namespace NotifyServer.Models.NetworkModels;

public class NotifyPackagePipeLineFilter : FixedHeaderPipelineFilter<NetworkPackage>
{
    public NotifyPackagePipeLineFilter() : base(NetworkPackage.HeaderSize)
    {
    }

    protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        
        reader.TryReadLittleEndian(out int bodyLength);
        reader.Advance(sizeof(int)); // Skip Key bytes
        
        return bodyLength;
    }

    protected override NetworkPackage DecodePackage(ref ReadOnlySequence<byte> buffer)
    {
        var reader = new SequenceReader<byte>(buffer);
        
        reader.TryReadLittleEndian(out int bodySize);
        reader.TryReadLittleEndian(out int key);
        
        var body = reader.Sequence.Slice(reader.Consumed, bodySize).ToArray();
        return new NetworkPackage(key, bodySize, body);
    }
}