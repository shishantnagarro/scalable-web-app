using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ScheduledCleanupFunction;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoClient;
    private readonly string _tableName;

    public Function()
    {
        _dynamoClient = new AmazonDynamoDBClient();
        _tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? "web-app-logs";
    }

    public async Task<string> FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation("Starting scheduled cleanup of old DynamoDB records");
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30).ToString("O");
            var deletedCount = 0;
            var scanRequest = new ScanRequest
            {
                TableName = _tableName,
                FilterExpression = "#ts < :cutoff",
                ExpressionAttributeNames = new Dictionary<string, string> { ["#ts"] = "Timestamp" },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { [":cutoff"] = new AttributeValue { S = cutoffDate } }
            };
            ScanResponse scanResponse;
            do
            {
                scanResponse = await _dynamoClient.ScanAsync(scanRequest);
                foreach (var item in scanResponse.Items)
                {
                    var deleteRequest = new DeleteItemRequest
                    {
                        TableName = _tableName,
                        Key = new Dictionary<string, AttributeValue> { ["Id"] = item["Id"] }
                    };
                    await _dynamoClient.DeleteItemAsync(deleteRequest);
                    deletedCount++;
                }
                scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
            } while (scanResponse.LastEvaluatedKey != null && scanResponse.LastEvaluatedKey.Count != 0);

            context.Logger.LogInformation($"Cleanup completed. Deleted {deletedCount} old records");
            return $"Successfully deleted {deletedCount} old records";
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error during cleanup: {ex.Message}");
            throw;
        }
    }
}
