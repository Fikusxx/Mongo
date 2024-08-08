using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("many")]
public sealed class Many : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Many()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        await db.InsertManyAsync([
            new Game { Id = Guid.NewGuid(), Title = "Ori" },
            new Game { Id = Guid.NewGuid(), Title = "Ori" },
            new Game { Id = Guid.NewGuid(), Title = "Ori" },
        ]);

        // await db.InsertManyAsync([
        //     new Game { Id = Guid.NewGuid() },
        //     new Game { Id = Guid.NewGuid() },
        //     new Game { Id = Guid.NewGuid() },
        // ]);

        return Ok();
    }

    [HttpPost]
    [Route("create-unordered")]
    public async Task<IActionResult> CreateUnordered()
    {
        // if isOrdered = true, which is TRUE by default
        // records will be inserted up until some insertion throws an error
        // If true: 1 - inserted, 2 - throws an error, 3 - wont be inserted
        // If false: 1 - inserted, 2 - throws an error, 3 - inserted

        var id = Guid.NewGuid();
        await db.InsertManyAsync([
            new Game { Id = id, Title = "Ori #1" },
            new Game { Id = id, Title = "Ori #2" },
            new Game { Id = Guid.NewGuid(), Title = "Ori #3" }
        ], new InsertManyOptions { IsOrdered = false });

        return Ok();
    }

    /// <summary>
    /// Most of update logic is at single/update route
    /// </summary>
    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> Update()
    {
        var update = Builders<Game>.Update.Set(x => x.Price, 123);
        var updateResult = await db.UpdateManyAsync(x => x.Title == "Ori", update);

        return Ok(updateResult);
    }

    [HttpGet]
    [Route("find-collection")]
    public async Task<IActionResult> GetCollection()
    {
        var filter = Builders<Game>.Filter.Eq(x => x.Title, "Ori");
        var resultWithFilter = await db.Find(filter).ToListAsync();

        var resultWithLambda = await db.Find(x => x.Title == "Ori").ToListAsync();

        var resultWithOptions = await db.Find(x => x.Title == "Ori", new FindOptions { BatchSize = 2 }).ToListAsync();

        return Ok(new { resultWithFilter, resultWithLambda, resultWithOptions });
    }

    [HttpGet]
    [Route("sorting")]
    public async Task<IActionResult> Sorting()
    {
        var sorting = Builders<Game>.Sort
            .Ascending(x => x.Title)
            .Descending(x => x.Price);

        var result = await db.Find(_ => true).Sort(sorting).ToListAsync();

        return Ok(result);
    }

    [HttpGet]
    [Route("queryable")]
    public async Task<IActionResult> Queryable()
    {
        List<Game> dummyGames =
        [
            new Game { Id = Guid.NewGuid(), Title = "Ori #1", Price = 123 },
            new Game { Id = Guid.NewGuid(), Title = "Ori #2", Price = 49 },
            new Game { Id = Guid.NewGuid(), Title = "Ori #3", Price = 19 },
        ];

        await db.InsertManyAsync(dummyGames);

        var result = db.AsQueryable()
            .Where(x => x.Title != null && x.Title.Contains("Ori"))
            .Where(x => x.Price < 50)
            .Select(x => new { x.Title, x.Price }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/aggregation/
    /// </summary>
    [HttpGet]
    [Route("aggregate")]
    public async Task<IActionResult> Aggregate()
    {
        List<Game> dummyGames =
        [
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 123 },
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 123 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 55 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 55 },
        ];

        await db.InsertManyAsync(dummyGames);

        // Aggregate approach
        var sortFilter = Builders<Game>.Sort.Ascending(x => x.Title);
        var matchFilter = Builders<Game>.Filter.Empty; // all objects, _ => true

        var builder = new EmptyPipelineDefinition<Game>()
            .Match(matchFilter)
            .Sort(sortFilter)
            .Group(x => x.Title,
                games => new
                {
                    Key = games.Key,
                    AvgPrice = games.Sum(x => x.Price)
                });

        var aggregateResult = await db.Aggregate(builder).ToListAsync();

        // LINQ approach
        var linqResult = db.AsQueryable()
            .Where(_ => true) // obsolete
            .OrderBy(x => x.Title)
            .GroupBy(x => x.Title, (key, games) => new
            {
                Key = key,
                AvgPrice = games.Sum(x => x.Price)
            })
            .ToList();

        return Ok(new { aggregateResult, linqResult });
    }

    /// <summary>
    /// Search commands only allowed on Atlas
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/atlas-search/#overview
    /// </summary>
    [HttpGet]
    [Route("searching")]
    public IActionResult Searching()
    {
        var result = db.Aggregate().Search(Builders<Game>.Search.Phrase(x => x.Title, "r")).ToList();

        return Ok(result);
    }
}