using System.Collections;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using SteamSleuthData.Cache;
using SteamSleuthData.Data.Models;
using SteamSleuthData.Data.Providers.Interfaces;

namespace SteamSleuthData.Data.Importers;

// TODO: Refactor this class to use even more gooder coding practices :^)
public class TestAddFromFileJSONLines
{
    public static async Task AddAllFromDirectoryMask(string directoryMask, IStoreAppDatabase storeAppDatabase)
    {
        // Usage: AddAllFromDirectoryMask(@"C:\Users\User\Desktop\data\converted\MONGO_IMPORT_StoreApps__json_db.blob.*");
        var dir = System.IO.Path.GetDirectoryName(directoryMask);
        var mask = System.IO.Path.GetFileName(directoryMask);
        var files = System.IO.Directory.GetFiles(dir, mask, SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            await AddAllFromFile(file, storeAppDatabase);
        }
    }

    public static async Task AddAllFromFile(string filename, IStoreAppDatabase storeAppDatabase)
    {
        // For each line in filename, add to database

        if (storeAppDatabase == null)
        {
            throw new NullReferenceException("Error, no database provided.");
        }

        // Get a list of every single StoreApp.steam_appid in the database
        var allStoreAppSteamAppIds = await storeAppDatabase.GetUniqueSteamAppIdsAsync();

        Hashtable allStoreAppSteamAppIdsHash = new Hashtable();

        const bool DEBUG_TEST_WITHOUT_DEDUPING = false; // Change this
        if (!DEBUG_TEST_WITHOUT_DEDUPING)
        {
            allStoreAppSteamAppIds.ForEach(x =>allStoreAppSteamAppIdsHash.Add(x.ToString(), true));
            allStoreAppSteamAppIds = null; // Free references for GC
        }
        else
        {
            Console.WriteLine("#### [DEBUG] DISABLING DE-DUPE CHECK - ATTEMPTING TO ADD ALL DUPLICATES TO DB ###");
            Thread.Sleep(500); //wait
        }


        var lines = File.ReadLinesAsync(filename, Encoding.UTF8); // ALWAYS USE UTF8
        int batchSize = 100;
        int batchCount = 0;
        int totalAdded = 0;
        var batch = new List<BsonDocument>();
        
        Func<Task> addBatch =  async () =>
        {
            try
            {
                if (batch.Count > 0)
                {
                    await storeAppDatabase.UpsertStoreAppsAsync<BsonDocument>(batch);
                }
            }
            catch (MongoBulkWriteException<BsonDocument> ex)
            {
                Console.WriteLine($"Error adding batch: {ex.Message}");
                foreach (var error in ex.WriteErrors)
                {
                    Console.WriteLine($"Error adding batch: {error.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding batch: {ex.Message}");
            }
            batch.Clear();
            batchCount = 0;
        };
        
        await foreach (var line in lines)
        {
            totalAdded++;
            if (totalAdded % 1000 == 0)
                Console.WriteLine($"Added {totalAdded} items");
            // Deserialize line into StoreAppModel
            var storeAppDocument = BsonDocument.Parse(line);
            // TODO: Add storeApp to database
            try
            {
                var appid = storeAppDocument["steam_appid"];
                if (appid?.ToString().Length > 0)
                {
                    if(allStoreAppSteamAppIdsHash.ContainsKey(appid.ToString()))
                        continue;

                    
                    //Console.WriteLine($"Adding storeApp to database: {name.ToString()}");

                    // Add to database
                    //await database.AddItemAsync("Steam_StoreApps", storeAppDocument);
                    batch.Add(storeAppDocument);
                    batchCount++;
                    if (batchCount >= batchSize)
                    {
                        await addBatch();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing storeApp: {ex.Message}");
            }


        }
        
        await addBatch(); // Add any remaining items
        
    }
}