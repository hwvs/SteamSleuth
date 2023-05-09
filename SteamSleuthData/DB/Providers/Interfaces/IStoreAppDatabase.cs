namespace SteamSleuthData.Data.Providers.Interfaces;

public interface IStoreAppDatabase
{
    Task<List<int>> GetUniqueSteamAppIdsAsync(bool forceRefreshCache = false);
    
    // Method to add more StoreApps
    Task<object> UpsertStoreAppsAsync<T>(List<T> appIds);
    Task<object> UpsertOneStoreAppAsync<T>(T appId);
}