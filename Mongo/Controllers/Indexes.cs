using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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

    /// <summary>
    /// https://www.mongodb.com/docs/manual/reference/command/explain/
    /// </summary>
    [HttpGet]
    [Route("explain")]
    public IActionResult Explain()
    {
        var whatToDo = new
        {
            DockerCommand = "docker exec -it mongodb mongosh",
            Database = "use db_name",
            ExplainQuery = "db.Games.explain(\"executionStats\").find({Price:{'$gt':100}})"
        };

        return Ok(whatToDo);
    }

    [HttpGet]
    [Route("index-options")]
    public IActionResult GetOptions()
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

        var indexOptions = new CreateIndexOptions
        {
            ExpireAfter = TimeSpan.FromSeconds(10),
        };

        return Ok(indexOptions);
    }

    [HttpPost]
    [Route("single-field")]
    public async Task<IActionResult> SingleField()
    {
        var index = Builders<Game>.IndexKeys.Ascending(x => x.Price);
        var indexOptions = new CreateIndexOptions { Name = "Price_777" };
        var indexModel = new CreateIndexModel<Game>(index, indexOptions);
        var indexName = await db.Indexes.CreateOneAsync(indexModel);

        return Ok(indexName);
    }

    [HttpPost]
    [Route("compound")]
    public async Task<IActionResult> Compound()
    {
        var index = Builders<Game>.IndexKeys
            .Ascending(x => x.Title)
            .Ascending(x => x.Price);

        var indexOptions = new CreateIndexOptions { Name = "compound_index" };
        var indexModel = new CreateIndexModel<Game>(index, indexOptions);
        var indexName = await db.Indexes.CreateOneAsync(indexModel);

        return Ok(indexName);
    }

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/indexes/#text-indexes
    /// https://www.mongodb.com/docs/manual/core/indexes/index-types/index-text/
    /// </summary>
    [HttpPost]
    [Route("text-search")]
    public async Task<IActionResult> CreateTextIndex()
    {
        // Collection can only contain 1! text index, thus create compound if you want multiple fields covered
        var textIndex = Builders<Game>.IndexKeys.Text(x => x.Title);
        var textIndexOptions = new CreateIndexOptions { Name = "Title_777" };
        var textIndexModel = new CreateIndexModel<Game>(textIndex, textIndexOptions);
        var indexName = await db.Indexes.CreateOneAsync(textIndexModel);

        return Ok(indexName);
    }

    [HttpGet]
    [Route("text-search")]
    public async Task<IActionResult> GetByTitle([FromQuery] string? title, [FromQuery] string? textSearch)
    {
        // insert dummy data
        await db.InsertManyAsync([
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
        ]);


        var filter = Builders<Game>.Filter.Text(textSearch ?? "Ori");
        var expression = Builders<Game>.Projection.Expression(x => new { x.Title, x.Price });
        var result = await db.Find(filter).Project(expression).ToListAsync();

        // text index is NOT used this way
        // var result = await db.Find(x => x.Title == "Ori").Project(expression).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }

    [HttpDelete]
    [Route("drop-all")]
    public async Task<IActionResult> DropAll()
    {
        await db.Indexes.DropAllAsync();

        return Ok();
    }
}