// See https://aka.ms/new-console-template for more information

// Powershell
/* 

if($named_apps -eq $null) {
    if($apps_raw -eq $null -or $apps_raw.Length -lt 1000000) {
        write-host "Downloading app list from Steam API"
        $apps_raw = (Invoke-WebRequest 'https://api.steampowered.com/ISteamApps/GetAppList/v2/')
    }
    write-host "Parsing JSON app list..."
    $apps = $apps_raw.Content | ConvertFrom-Json
    write-host "Filtering out apps with no name..."
    $named_apps = $apps.applist.apps | Where-Object { $_.name.ToString().Trim().Length -gt 0 }
}*/

// C#


using System.Text;
using System.Text.Json;
using MongoDB.Bson.IO;
using SteamSleuthScraper;
using SteamSleuthScraper.Net;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var allApps = await SteamAPI.GetAppListAsync();
foreach(var app in allApps)
{
    Console.WriteLine($"app.AppId: {app.AppId}, app.Name: {app.Name}");
}