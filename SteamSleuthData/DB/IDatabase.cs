using MongoDB.Driver;
using SteamSleuthData.Data.Providers;
using SteamSleuthData.Data.Providers.Interfaces;

namespace SteamSleuthData.Data;

public interface IDatabase
{
    // TODO: Future use to decouple from MongoDB
    
    public StoreAppDatabaseProvider GetStoreAppDatabase();
    
}