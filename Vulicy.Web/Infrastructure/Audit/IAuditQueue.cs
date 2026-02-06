using System.Threading.Channels;
using Amazon.DynamoDBv2.Model;

namespace Vulicy.Web.Infrastructure;

public interface IAuditQueue
{
    void Enqueue(Dictionary<string, AttributeValue> auditEvent);
    ValueTask<Dictionary<string, AttributeValue>> DequeueAsync(CancellationToken cancellationToken);
}

public class AuditQueue : IAuditQueue
{
    private readonly Channel<Dictionary<string, AttributeValue>> _channel;

    public AuditQueue()
    {
        _channel = Channel.CreateBounded<Dictionary<string, AttributeValue>>(new BoundedChannelOptions(256) { FullMode = BoundedChannelFullMode.DropWrite });
    }

    public void Enqueue(Dictionary<string, AttributeValue> auditEvent)
    {
        _channel.Writer.TryWrite(auditEvent);
    }

    public ValueTask<Dictionary<string, AttributeValue>> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}