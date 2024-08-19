using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("text")]
public class Text : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Text()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }
    
    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> GetByTitle([FromQuery] string? title, [FromQuery] string? textSearch)
    {
        // insert dummy data
        await db.InsertManyAsync([
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
            new Game { Id = Guid.NewGuid(), Title = title ?? "Ori" },
        ]);

        var options = new TextSearchOptions { Language = "english", CaseSensitive = false };
        var filter = Builders<Game>.Filter.Text(textSearch ?? "Ori", options);

        // and other props..
        var projection = Builders<Game>.Projection.Expression(x => new { x.Title, x.Price });
        var result = await db.Find(filter).Project(projection).ToListAsync();

        // text index is NOT used this way
        // var result = await db.Find(x => x.Title == "Ori").Project(expression).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }

    /// <summary>
    /// exclude words should be: -true -indeed -red ...
    /// </summary>
    [HttpGet]
    [Route("search-with-score")]
    public async Task<IActionResult> GetWithScore([FromQuery] string? textSearch, [FromQuery] string? excludeWords)
    {
        await db.InsertManyAsync([
            new Game { Id = Guid.NewGuid(), Title = "red book" },
            new Game { Id = Guid.NewGuid(), Title = "true, my awesome red book" },
            new Game { Id = Guid.NewGuid(), Title = "indeed, books that are red are awesome" },
        ]);

        var filter = Builders<Game>.Filter.Text(textSearch + " " + excludeWords);

        // and other props..
        var projection = Builders<Game>.Projection.MetaTextScore("my_score")
            .Exclude(x => x.Id).Include(x => x.Title);

        // sorted desc by default
        var sort = Builders<Game>.Sort.MetaTextScore("my_score");
        var result = await db.Find(filter).Project<dynamic>(projection).Sort(sort).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }
}