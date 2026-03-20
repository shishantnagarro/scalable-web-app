using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SNSProcessorFunction;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public Function()
    {
        _dynamoClient = new AmazonDynamoDBClient();
        _tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? "web-app-logs";
    }

    public async Task<string> FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
    {
        foreach (var record in snsEvent.Records)
        {
            await ProcessSNSRecord(record, context);
        }
        return "Successfully processed SNS messages";
    }

    private async Task ProcessSNSRecord(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing SNS message: {record.Sns.Message}");
            var logEntry = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = Guid.NewGuid().ToString() },
                ["Action"] = new AttributeValue { S = "sns_message" },
                ["Details"] = new AttributeValue { S = record.Sns.Message },
                ["Timestamp"] = new AttributeValue { S = DateTime.UtcNow.ToString("O") }
            };
            var request = new PutItemRequest { TableName = _tableName, Item = logEntry };
            await _dynamoClient.PutItemAsync(request);
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing SNS record: {ex.Message}");
            throw;
        }
    }
}
