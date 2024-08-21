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
            new Game { Id = Guid.NewGuid(), Title = "Ori" },
            new Game { Id = Guid.NewGuid(), Title = "Ori" },
        ]);

        return Ok();
    }

    [HttpPost]
    [Route("create-unordered")]
    public async Task<IActionResult> CreateUnordered()
    {
        // default - true
        // defines if records will be inserted up until some insertion throws an error
        // true: 1 - inserted, 2 - throws an error, 3 - wont be inserted
        // false: 1 - inserted, 2 - throws an error, 3 - inserted

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

        var resultWithOptions = await db.Find(x => x.Title == "Ori", new FindOptions { BatchSize = 1 }).ToListAsync();

        var resultWithLinq = db.AsQueryable().Where(x => x.Title == "Ori").ToList();

        return Ok(new { resultWithFilter, resultWithLambda, resultWithOptions, resultWithLinq });
    }

    [HttpGet]
    [Route("pagination")]
    public async Task<IActionResult> Pagination()
    {
        var native = await db.Find(x => x.Title == "Ori")
            .Skip(0)
            .Limit(2)
            .ToListAsync();

        var linq = db.AsQueryable().Where(x => x.Title == "Ori")
            .Skip(0)
            .Take(2)
            .ToList();

        return Ok(new { native, linq });
    }
}