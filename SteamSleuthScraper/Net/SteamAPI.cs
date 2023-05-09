using SteamSleuthData.Cache;
using System.Text.Json;

namespace SteamSleuthScraper.Net;

public class SteamAPI
{
    // /GetAppList/
    private const string GetAppListUrl = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";
    private const string FileAppList = "AppList.json";

    /// <summary>
    /// Gets the app list from the Steam API, or from a cached file if it exists. 
    /// The file will be deleted after 4 hours, or if the API request fails.
    /// 
    /// The cache will be deleted after 6 hours.
    ///
    /// </summary>
    /// <param name="allowCache"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<List<SteamAppModel>?> GetAppListAsync(bool allowCache = true)
    {
        Stream jsonStream = null;

        if(allowCache)
        {
            var cached = ObjectCacheProvider.GetBucket<List<SteamAppModel>>(CacheBucketKey.GetAppList_ListSteamAppModel);
            if(cached != null)
                return cached;
        }
            
        
        if(File.Exists(FileAppList))
        {
            var fileInfo = new FileInfo(FileAppList);
            if(fileInfo.LastWriteTimeUtc > DateTime.UtcNow.AddHours(-4)) // 4 hours 
            {
                Console.WriteLine("Reading apps list from file");
                jsonStream = File.OpenRead(FileAppList);
            }
        }

        if (jsonStream == null) // HTTP fallback
        {
            Console.WriteLine("Reading apps list from Steam API");
            jsonStream = await Request.GetAsStreamAsync(GetAppListUrl);

            Console.WriteLine("Writing to file");
            //File.WriteAllText(FileAppList, jsonStream);
            using (var fileStream = File.Create(FileAppList))
            {
                await jsonStream.CopyToAsync(fileStream);
                jsonStream.Position = 0;
            }
        }

        if(jsonStream == null)
            throw new Exception("jsonStream is null");
        
        var appsList = await JsonSerializer.DeserializeAsync<__ApplistModel>(jsonStream);
        var allApps = await appsList.GetAllNamedAppsAsync();

        if(allApps != null)
            ObjectCacheProvider.SetBucket(key: CacheBucketKey.GetAppList_ListSteamAppModel, value: allApps, ttlSeconds:3600*6 /* 6 hrs */);

        return allApps;
    }
}