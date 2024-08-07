using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("indexes")]
public sealed class Indexes : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Indexes()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        // Properties of an index
        // createIndexOptions.Background = options.Background;
        // createIndexOptions.Bits = options.Bits;
        // createIndexOptions.BucketSize = options.BucketSize;
        // createIndexOptions.Collation = options.Collation;
        // createIndexOptions.DefaultLanguage = options.DefaultLanguage;
        // createIndexOptions.ExpireAfter = options.ExpireAfter;
        // createIndexOptions.Hidden = options.Hidden;
        // createIndexOptions.LanguageOverride = options.LanguageOverride;
        // createIndexOptions.Max = options.Max;
        // createIndexOptions.Min = options.Min;
        // createIndexOptions.Name = options.Name;
        // createIndexOptions.Sparse = options.Sparse;
        // createIndexOptions.SphereIndexVersion = options.SphereIndexVersion;
        // createIndexOptions.StorageEngine = options.StorageEngine;
        // createIndexOptions.TextIndexVersion = options.TextIndexVersion;
        // createIndexOptions.Unique = options.Unique;
        // createIndexOptions.Version = options.Version;
        // createIndexOptions.Weights = options.Weights;

        var index = Builders<Game>.IndexKeys.Ascending(x => x.Price);
        var indexOptions = new CreateIndexOptions { Name = "Price_777" };
        var indexModel = new CreateIndexModel<Game>(index, indexOptions);
        var indexName = await db.Indexes.CreateOneAsync(indexModel);

        return Ok(indexName);
    }

    [HttpDelete]
    [Route("drop-all")]
    public async Task<IActionResult> DropAll()
    {
        await db.Indexes.DropAllAsync();

        return Ok();
    }
}