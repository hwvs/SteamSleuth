using MongoDB.Bson;
using MongoDB.Driver;
using SteamSleuthData.Cache;
using SteamSleuthData.Data.Engines;
using SteamSleuthData.Data.Providers.Interfaces;

namespace SteamSleuthData.Data.Providers;

public class StoreAppDatabaseProvider : IStoreAppDatabase
{
    // Collection of scraped Steam StoreApps from their API
    private string mongo_collectionName = "Steam_StoreApps";
    private IDatabase _database;

    public StoreAppDatabaseProvider(IDatabase database)
    {
        _database = database;
    }

    /*
    public async Task InsertOneStoreAppAsync<T>(T appId)
    {
        if (_database is MongoDatabase mongoDatabase)
        {
            if (false == typeof(BsonDocument).Equals(typeof(T)))
            {
                throw new ArgumentException("InsertOneStoreAppAsync MUST use the BsonDocument type for MongoDB");
                return;
            }
            
            await mongoDatabase.GetCollection<BsonDocument>(mongo_collectionName).InsertOneAsync(appId as BsonDocument,
                new InsertOneOptions()
                {
                    
                });
        }
        else
        {
            throw new NotImplementedException("Database type not supported.");
        }
    }
    

    public async Task InsertStoreAppsAsync<T>(List<T> appIds)
    {
        if (_database is MongoDatabase mongoDatabase)
        {
            if (false == typeof(BsonDocument).Equals(typeof(T)))
            {
                throw new ArgumentException("InsertStoreAppsAsync MUST use the BsonDocument type for MongoDB");
                return;
            }
            
            await mongoDatabase.GetCollection<BsonDocument>(mongo_collectionName).InsertManyAsync(appIds as List<BsonDocument>,
                new InsertManyOptions()
                {
                    IsOrdered = false // Ignore errors
                });
        }
        else
        {
            throw new NotImplementedException("Database type not supported.");
        }
    }
    */
    
    public async Task<object> UpsertOneStoreAppAsync<T>(T appId)
    {
        // Just pipe it away to the other method
        return await UpsertStoreAppsAsync<T>(new List<T>() {appId});
    }
    

    public async Task<object> UpsertStoreAppsAsync<T>(List<T> appIds)
    {
        if (_database is MongoDatabase mongoDatabase)
        {
            if (false == typeof(BsonDocument).Equals(typeof(T)))
            {
                throw new ArgumentException("UpsertStoreAppsAsync MUST use the BsonDocument type for MongoDB");
                return null;
            }
            
            var appIdsAsListBsonDoc = appIds as List<BsonDocument>;
            if (appIdsAsListBsonDoc == null)
                throw new NullReferenceException(
                    "UpsertStoreAppsAsync - appId doesn't convert inherit from List<BsonDocument>, but DB is MongoDatabase!");
            
            /*
            await mongoDatabase.GetCollection<BsonDocument>(mongo_collectionName).InsertManyAsync(appIdsAsListBsonDoc,
                new InsertManyOptions()
                {
                    IsOrdered = false // Ignore errors
                });
                */

            // TODO: Figure out how to do this in a cleaner way (hook on validation/schema?) - a func for
            // TODO: (cont) every app used to build a filter is *slightly* inefficient
            var filterBuilderFunc =  (BsonDocument updated) =>
            {
                return Builders<BsonDocument>.Filter.Where((doc) => doc["steam_appid"].Equals(updated["steam_appid"]));
            };
            
            // Always update the updated time, doing that in the DB :^)
            appIdsAsListBsonDoc.ForEach(doc => doc["_last_updated"] = new BsonDateTime(DateTime.Now));
            
            // Put result into a var before return to make setting breakpoints easier, compiles to same IL instructions
            var result = await mongoDatabase.BulkUpsert<BsonDocument>(collectionName:mongo_collectionName, appIdsAsListBsonDoc, filterBuilderFunc);
            return result;

        }
        else
        {
            throw new NotImplementedException($"Database type not supported. ({_database.GetType().ToString()})");
        }
    }

    public async Task<List<int>> GetUniqueSteamAppIdsAsync(bool forceRefreshCache = false)
    {
        const int CACHE_EXPIRY_SECONDS_STEAMAPPIDS = 30; // 30 Seconds
        
        if (!forceRefreshCache)
        {
            // Check the object cache :)
            var cached = ObjectCacheProvider.GetBucket<List<int>>(CacheBucketKey.AllAppIDs_ListInt);
            if (cached != null)
                return cached;
        }
        
        var property = "steam_appid";
        if (_database is MongoDatabase mongoDatabase)
        {
            // Debug/Protection, can be removed in prod
            #if DEBUG
                if(!mongoDatabase.Name.Equals("SteamSleuth"))
                    throw new Exception("Database must be named SteamSleuth. This is to prevent accidental overwrites of other databases.");
            #endif

            // Get a list of every single StoreApp.steam_appid in the database
            var result = await mongoDatabase.GetPropertyFromAllDocuments<int>(mongo_collectionName, property);
            if (result != null)
            {
                ObjectCacheProvider.SetBucket(CacheBucketKey.AllAppIDs_ListInt, result, ttlSeconds: CACHE_EXPIRY_SECONDS_STEAMAPPIDS);
            }

            return result;
        }
        else
        {
            throw new NotImplementedException("Database type not supported.");
        }
    }

    
}