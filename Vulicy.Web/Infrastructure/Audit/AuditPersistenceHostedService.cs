using Amazon.DynamoDBv2;

namespace Vulicy.Web.Infrastructure;

public class AuditPersistenceHostedService : BackgroundService
{
    private readonly IAuditQueue _auditQueue;
    private readonly AwsConfig _awsConfig;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly ILogger<AuditPersistenceHostedService> _logger;

    public AuditPersistenceHostedService(
        IAuditQueue auditQueue,
        AwsConfig awsConfig,
        IAmazonDynamoDB dynamoDbClient,
        ILogger<AuditPersistenceHostedService> logger)

    {
        _auditQueue = auditQueue;
        _awsConfig = awsConfig;
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var auditEvent = await _auditQueue.DequeueAsync(stoppingToken);
            if (!string.IsNullOrEmpty(_awsConfig.AuditTable))
            {
                try
                {
                    await _dynamoDbClient.PutItemAsync(_awsConfig.AuditTable, auditEvent, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to persist audit event");
                }
            }
        }
    }
}