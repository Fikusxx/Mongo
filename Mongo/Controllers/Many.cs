using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
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
            new Game { Id = Guid.NewGuid(), Title = "Ori" }
        ]);

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

        return Ok(new { resultWithFilter, resultWithLambda });
    }

    [HttpGet]
    [Route("sorting")]
    public async Task<IActionResult> Sorting()
    {
        var sorting = Builders<Game>.Sort.Ascending(x => x.Price);

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
            .Where(x => x.Title.Contains("Ori"))
            .Where(x => x.Price < 50)
            .Select(x => new { x.Title, x.Price }).ToList();

        return Ok(result);
    }
    
    /// <summary>
    /// TODO
    /// </summary>
    [HttpGet]
    [Route("searching")]
    public IActionResult Searching()
    {
        // Aggregation search commands only allowed on Atlas
        // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/atlas-search/
        var result = db.Aggregate().Search(Builders<Game>.Search.Phrase(x => x.Title, "r")).ToList();

        return Ok(result);
    }

    /// <summary>
    /// TODO
    /// </summary>
    [HttpGet]
    [Route("joining")]
    public IActionResult Joining()
    {
        // Lookup<TForeignDocument, TNewResult>(string foreignCollectionName,
        // FieldDefinition<TResult> localField,
        // FieldDefinition<TForeignDocument> foreignField,
        // FieldDefinition<TNewResult> @as,
        // AggregateLookupOptions<TForeignDocument,
        // TNewResult> options = null);

        //var result = db.Aggregate().Lookup<TForeignDocument, TNewResult>("otherCollection",
        //	game => game.PlayerId, player => player.Id, new { props });

        return Ok();
    }
}