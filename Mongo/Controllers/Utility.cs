using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
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
    [Route("count")]
    public async Task<IActionResult> GetCount()
    {
        var estimated = await db.EstimatedDocumentCountAsync(); // #1, fastest, uses collection meta data

        var withOptions =
            await db.CountDocumentsAsync(_ => true, new CountOptions { Hint = "_id_" }); // #2, uses only index scan

        var filter = Builders<Game>.Filter.Eq(x => x.Title, "Ori");
        var preBuiltFilter = await db.CountDocumentsAsync(filter); // slow

        var lambdaFilter = await db.CountDocumentsAsync(x => x.Title == "Ori"); // slow

        return Ok(new { preBuiltFilter, lambdaFilter, estimated, withOptions });
    }

    [HttpDelete]
    [Route("purge")]
    public async Task<IActionResult> PurgeCollection()
    {
        await db.DeleteManyAsync(_ => true);

        return Ok();
    }
}