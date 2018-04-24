using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using my_netcore_application.Entities;
using my_netcore_application.Interfaces;

namespace my_netcore_application.Repository
{
    public class FavoriteRepository : IFavoriteRepository
    {
        public const string TABLE_NAME = "favorites-jobs-table-dev";
        public const string INDEX_NAME = "job-index-dev";
        
        private readonly IAmazonDynamoDB _dynamoDBclient;
        private readonly DynamoDBContext _dynamoDBcontext;
        
        // https://docs.aws.amazon.com/es_es/amazondynamodb/latest/developerguide/CRUDHighLevelExample1.html
        // https://docs.aws.amazon.com/es_es/amazondynamodb/latest/developerguide/QueryMidLevelDotNet.html
        // https://docs.aws.amazon.com/es_es/amazondynamodb/latest/developerguide/LowLevelDotNetQuerying.html

        public FavoriteRepository(IAmazonDynamoDB dynamoDB)
        {
            _dynamoDBclient = dynamoDB;
            _dynamoDBcontext = new DynamoDBContext(dynamoDB);
        }

        public async Task<FavoriteEntity> GetFavoriteByIdAsync(string userId, int jobId)
        {
            return await _dynamoDBcontext.LoadAsync<FavoriteEntity>(userId, jobId);
        }

        public async void DeleteFavoriteByIdAsync(string userId, int jobId)
        {
            await _dynamoDBcontext.DeleteAsync<FavoriteEntity>(userId, jobId);
        }

        public async void CreateFavoriteAsync(FavoriteEntity favoriteItem)
        {
            await _dynamoDBcontext.SaveAsync(favoriteItem);
        }

        // https://docs.aws.amazon.com/es_es/amazondynamodb/latest/developerguide/LowLevelDotNetQuerying.html
        public async Task<FavoritesListEntity> GetFavoritesAsync(string userId, int pageSize = 50, int? jobId = null)
        {
            var request = new QueryRequest
            {
                TableName = TABLE_NAME,
                IndexName = INDEX_NAME,
                Limit = pageSize,
                KeyConditionExpression = "userId = :uid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":uid", new AttributeValue { S =  userId } }
                },
                ProjectionExpression = "jobId, jobTitle"
                //ConsistentRead = true
            };

            if (jobId != null && jobId > 0)
            {
                // paginación
                request.ExclusiveStartKey = new Dictionary<string, AttributeValue>
                {
                    { "userId", new AttributeValue { S =  userId } },
                    { "jobId", new AttributeValue { N =  jobId.ToString() } }
                };
            }

            var response = await _dynamoDBclient.QueryAsync(request);

            return new FavoritesListEntity 
            { 
                Items = ConvertToFavoritesEntities(response.Items),
                LastUserId = GetUserId(response),
                LastJobId = GetJobId(response)
            };
        }

        private List<FavoriteEntity> ConvertToFavoritesEntities(List<Dictionary<string, AttributeValue>> items)
        {
            var favorites = new List<FavoriteEntity>();
            foreach (Dictionary<string, AttributeValue> item in items)
                favorites.Add(ConvertToFavoriteEntity(item));

            return favorites;                
        }

        private FavoriteEntity ConvertToFavoriteEntity(Dictionary<string, AttributeValue> item)
        {
            return new FavoriteEntity 
                    { 
                        JobId = int.Parse(item["jobId"].N),
                        JobTitle = item["jobTitle"].S
                    };
        }

        private string GetUserId(QueryResponse response)
        {
            return response.LastEvaluatedKey.Count > 0 ? response.LastEvaluatedKey["userId"].S : "";
        }

        private int? GetJobId(QueryResponse response)
        {
            return response.LastEvaluatedKey.Count > 0 ? int.Parse(response.LastEvaluatedKey["jobId"].N) as int? : null;
        }
        
    }
}