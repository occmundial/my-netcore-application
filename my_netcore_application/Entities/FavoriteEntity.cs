using Amazon.DynamoDBv2.DataModel;

// https://docs.aws.amazon.com/es_es/amazondynamodb/latest/developerguide/DotNetSDKHighLevel.html
namespace my_netcore_application.Entities
{
    [DynamoDBTable("favorites-jobs-table-dev")]
    public class FavoriteEntity
    {        
        [DynamoDBHashKey("userId")] // Partition key   
        public string UserId { get; set; }

        [DynamoDBRangeKey("jobId")] // Sort key
        public int JobId {get; set;}

        [DynamoDBProperty("jobTitle")]    
        public string JobTitle { get; set; }        
    }
}