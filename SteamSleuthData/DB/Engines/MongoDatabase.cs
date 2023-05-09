using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SteamSleuthData.Data.Providers;
using SteamSleuthData.Data.Providers.Interfaces;

namespace SteamSleuthData.Data.Engines
{

    public class MongoDatabase : IDatabase
    {
        private readonly IMongoDatabase _database;
        public string Name { get; private set; } = String.Empty;
        
        public StoreAppDatabaseProvider GetStoreAppDatabase()
        {
            return new StoreAppDatabaseProvider(this);
        }

        private MongoDatabase(IMongoDatabase database)
        {
            _database = database;
            Name = database.DatabaseNamespace.DatabaseName;
        }
        
        private static string GetConnectionString()
        {
            var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "mongodb://localhost:27017";
            }
            return connectionString;
        }
        
        public static async Task<MongoDatabase> ConnectAsync(string databaseName, int maxConnectionPoolSize = 100)
        {
            var connectionString = GetConnectionString();
            return await ConnectAsync(connectionString, databaseName, maxConnectionPoolSize);
        }

        private static async Task<MongoDatabase> ConnectAsync(string connectionString, string databaseName, int maxConnectionPoolSize = 100)
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.MaxConnectionPoolSize = maxConnectionPoolSize;
            settings.WaitQueueSize = 1000;
            var client = new MongoClient(settings);
            var database = client.GetDatabase(databaseName);
            await EnsureIndexesAsync(database);
            return new MongoDatabase(database);
        }

        public async Task<BulkWriteResult<T>> BulkUpsert<T>(string collectionName, List<T> records, Func<T, FilterDefinition<T>> filterBuilderFunc)
        {
            var bulkOps = new List<WriteModel<T>>();
            foreach (var record in records)
            {
                var upsertOne = new ReplaceOneModel<T>(
                        filterBuilderFunc(record),
                        record)
                    { IsUpsert = true };
                bulkOps.Add(upsertOne);
            }
            var collection = _database.GetCollection<T>(collectionName);
            return await collection.BulkWriteAsync(bulkOps);
        }

        public async Task<List<T>> GetPropertyFromAllDocuments<T>(string collectionName, string propertyName)
        {
            // SELECT propertyName FROM collectionName
            var collection = _database.GetCollection<BsonDocument>(collectionName);
            var filter = new BsonDocument();
            var projection = Builders<BsonDocument>.Projection.Include(propertyName);
            
            /*
            var cursor = await collection.FindAsync(filter, new FindOptions<BsonDocument, BsonDocument>()
            {
                Projection = projection
            });
            */
            var cursor = await collection.DistinctAsync<T>(propertyName, filter);
            //Console.WriteLine(cursor.ToJson());
            var results = new List<T>();
            while (await cursor.MoveNextAsync())
            {
                var batch = cursor.Current;
                foreach (var document in batch)
                {
                    results.Add(document);
                }
            }
            return results;
        }

        private static async Task EnsureIndexesAsync(IMongoDatabase database)
        {
            // Add any index creation code here
            // Example: await database.CollectionName.CreateIndexAsync("FieldName");
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        public async Task DropCollectionAsync(string collectionName)
        {
            await _database.DropCollectionAsync(collectionName);
        }

        public async Task<bool> CollectionExistsAsync(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        public async Task CreateCollectionAsync<T>(string collectionName)
        {
            var options = new CreateCollectionOptions
            {
                AutoIndexId = true,
                MaxSize = 1000000,
                Capped = true,
                MaxDocuments = 10000
            };
            await _database.CreateCollectionAsync(collectionName, options);
        }

        public async Task DropCollectionAsync<T>(string collectionName)
        {
            await _database.DropCollectionAsync(collectionName);
        }

        public async Task AddItemAsync<T>(string collectionName, T item)
        {
            var collection = _database.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(item);
        }
        
    }
}