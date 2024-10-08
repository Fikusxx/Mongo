using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Configuration;

namespace Mongo.Controllers;

/// <summary>
/// https://www.mongodb.com/docs/drivers/csharp/current/faq/#how-does-connection-pooling-work-in-the-.net-c--driver-
/// </summary>
public sealed class Connection
{
    private void PrivateDeployment()
    {
        var credential = MongoCredential.CreateCredential(databaseName: "db", username: "name", password: "pw");

        // Connection Options https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connection-options/
        var settings = new MongoClientSettings
        {
            Scheme = ConnectionStringScheme.MongoDB,
            Server = new MongoServerAddress("localhost", 27017),
            ServerApi = new ServerApi(ServerApiVersion
                .V1), // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/stable-api/
            MaxConnectionPoolSize = 100, // default
            MaxConnectionLifeTime = TimeSpan.FromMinutes(30), // default
            Credential = credential,

            // https://www.mongodb.com/docs/manual/reference/write-concern/
            // w - on how many instances (shards) write should be acknowledged, default "majority"
            // journal - WAL basically, default undefined. If "undefined" then its logic based on "w" value and if journaling is enabled
            // wTimeout - timeout you give server to respond on a write operation, default 200ms
            // fsync - ???
            WriteConcern = new WriteConcern(w: 1, wTimeout: TimeSpan.FromMilliseconds(200), fsync: null, journal: true),

            // https://www.mongodb.com/docs/manual/reference/read-concern/
            // kinda line an isolation level on reads in rdbms
            ReadConcern = new ReadConcern(level: ReadConcernLevel.Majority)
        };

        // should be created ONCE for each process (app), i.e singleton
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
        var db = client.GetDatabase("admin").GetCollection<BsonDocument>("???");
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