// See https://aka.ms/new-console-template for more information

using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SteamSleuthData.Data;
using SteamSleuthData.Data.Engines;
using SteamSleuthData.Data.Importers;
using SteamSleuthData.Data.Models;
Console.OutputEncoding = Encoding.UTF8; // Always UTF8


MongoDatabase database = await MongoDatabase.ConnectAsync("SteamSleuth");


/*
// Get unknown type, first document
var allDocuments = await database.GetCollection<BsonDocument>("Steam_StoreApps").FindAsync(
    new BsonDocument(), 
    new FindOptions<BsonDocument, BsonDocument> { Limit = 100 }
);

// Deserialize to StoreAppModel, test
foreach(var document in allDocuments.ToEnumerable())
{
    var storeAppModel = BsonSerializer.Deserialize<StoreAppModel>(document);

    Console.WriteLine(storeAppModel);
}*/

var ids = await database.GetStoreAppDatabase().GetUniqueSteamAppIdsAsync();

await TestAddFromFileJSONLines.AddAllFromDirectoryMask(@"C:\Users\User\Desktop\data\converted\ADDED\MONGO_IMPORT_StoreApps__json_db.blob*", database.GetStoreAppDatabase());