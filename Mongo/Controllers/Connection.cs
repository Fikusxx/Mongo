using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Configuration;

namespace Mongo.Controllers;

public sealed class Connection
{
    private void PrivateDeployment()
    {
        // Connection Options https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connection-options/
        var settings = new MongoClientSettings
        {
            Scheme = ConnectionStringScheme.MongoDB,
            Server = new MongoServerAddress("localhost", 27017),
            ServerApi = new ServerApi(ServerApiVersion.V1), // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/stable-api/
        };

        var client = new MongoClient(settings);
        var db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connect/#std-label-csharp-connect-to-mongodb
    /// </summary>
    private void AtlasDeployment()
    {
        const string connectionUri = "<connection string>";
        var settings = MongoClientSettings.FromConnectionString(connectionUri);
        settings.ServerApi = new ServerApi(ServerApiVersion.V1);
        var client = new MongoClient(settings);

        try
        {
            var result = client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void Compression()
    {
        var settings = new MongoClientSettings()
        {
            Scheme = ConnectionStringScheme.MongoDB,
            Server = new MongoServerAddress("<cluster-url>"),
            Compressors = new List<CompressorConfiguration>
            {
                new CompressorConfiguration(CompressorType.Snappy),
                new CompressorConfiguration(CompressorType.Zlib),
                new CompressorConfiguration(CompressorType.ZStandard),
            }
        };
        var client = new MongoClient(settings);
    }
}