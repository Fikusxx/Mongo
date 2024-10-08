using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("utility")]
public sealed class Utility : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Utility()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }

    [HttpGet]
    [Route("ping")]
    public IActionResult Ping()
    {
        try
        {
            db.Database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }

    [HttpGet]
    [Route("stats")]
    public async Task<IActionResult> Stats()
    {
        var command = new BsonDocument { { "dbStats", 1 }, { "scale", 1 } };
        var stats = await db.Database.RunCommandAsync<BsonDocument>(command);

        return Ok(stats.ToJson());
    }

    [HttpGet]
    [Route("count")]
    public async Task<IActionResult> GetCount()
    {
        var estimated = await db.EstimatedDocumentCountAsync(); // #1, fastest, uses collection meta data

        var withOptions =
            await db.CountDocumentsAsync(_ => true, new CountOptions { Hint = "_id_" }); // #2, uses index scan

        var filter = Builders<Game>.Filter.Eq(x => x.Title, "Ori");
        var preBuiltFilter = await db.CountDocumentsAsync(filter); // slow

        var lambdaFilter = await db.CountDocumentsAsync(x => x.Title == "Ori"); // slow

        return Ok(new { preBuiltFilter, lambdaFilter, estimated, withOptions });
    }

    [HttpGet]
    [Route("collections")]
    public IActionResult GetCollections()
    {
        var result = db.Database.ListCollections()
            .ToList()
            .Select(x => x.ToString())
            .ToList();

        return Ok(result);
    }
}