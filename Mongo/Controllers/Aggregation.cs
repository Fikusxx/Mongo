using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("aggregation")]
public class Aggregation : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Aggregation()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
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

        var native = await db.Aggregate(builder).ToListAsync();

        var linq = db.AsQueryable()
            .Where(_ => true) // obsolete
            .OrderBy(x => x.Title)
            .GroupBy(x => x.Title, (key, games) => new
            {
                Key = key,
                AvgPrice = games.Sum(x => x.Price)
            })
            .ToList();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(new { native, linq });
    }

    [HttpGet]
    [Route("bucket")]
    public async Task<IActionResult> Bucket()
    {
        List<Game> dummyGames =
        [
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 8 },
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 17 },
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 25 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 11 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 19 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 31 },
        ];

        await db.InsertManyAsync(dummyGames);

        var bucketPipe = new EmptyPipelineDefinition<Game>()
            .Bucket(groupBy: x => x.Price,
                boundaries: [5, 10, 15, 20, 25, 30, 35],
                output: x => new
                {
                    Key = x.Key,
                    Average = x.Average(game => game.Price),
                    Titles = x.Select(game => game.Title)
                });

        var bucket = await db.Aggregate(bucketPipe).ToListAsync();
        
        var bucketAutoPipe = new EmptyPipelineDefinition<Game>()
            .BucketAuto(groupBy: x => x.Price,
                buckets: 5,
                output: x => new
                {
                    Key = x.Key,
                    Average = x.Average(game => game.Price),
                    Titles = x.Select(game => game.Title)
                });

        var autoBucket = await db.Aggregate(bucketAutoPipe).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(new { bucket, autoBucket });
    }
}