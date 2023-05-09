namespace SteamSleuthScraper;

using System.Text.Json.Serialization;

// { applist: { apps: [ { appid: 1, name: "Counter-Strike" }, ... ] } }
public class __ApplistModel
{
    [JsonPropertyName("applist")]
    public __AppsModel? Applist { get; set; }
    
    public async Task<List<SteamAppModel>> GetAllNamedAppsAsync()
    {
        await Task.Yield();
        
        var namedApps = new List<SteamAppModel>(1024);
        foreach(var app in Applist.Apps)
        {
            if(app.Name != null && app.Name.Trim().Length > 0)
                namedApps.Add(app);
            
        }
        return namedApps;
    } 
}

public class __AppsModel
{
    [JsonPropertyName("apps")]
    public List<SteamAppModel>? Apps { get; set; }
}

public class SteamAppModel
{
    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}