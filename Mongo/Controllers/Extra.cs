using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("extra")]
public class Extra : ControllerBase
{
    private readonly IMongoCollection<Game> db;
    
    public Extra()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }
    
    [HttpGet]
    [Route("projection")]
    public async Task<IActionResult> Projection()
    {
        // id property is auto included, if not specified explicitly
        var projection = Builders<Game>.Projection.Exclude(x => x.Id).Include(x => x.Title).Include(x => x.Price);

        // Id field is automatically excluded, unless specified explicitly
        var expression = Builders<Game>.Projection.Expression(x => new { x.Title, x.Price });

        // class for projection (<T>) should have ALL properties for given "Includes" AND also an Id property (unless excluded), otherwise it will throw
        var nativeProjection = await db.Find(_ => true).Project<dynamic>(projection).ToListAsync();

        // expression works as is
        var nativeExpression = await db.Find(_ => true).Project(expression).ToListAsync();

        var linq = db.AsQueryable()
            .Select(x => new { x.Title, x.Price })
            .ToList();

        return Ok(new { nativeProjection, nativeExpression, linq });
    }
    
    [HttpGet]
    [Route("sorting")]
    public async Task<IActionResult> Sorting()
    {
        var sorting = Builders<Game>.Sort
            .Ascending(x => x.Title)
            .Descending(x => x.Price);

        var native = await db.Find(_ => true).Sort(sorting).ToListAsync();

        var linq = db.AsQueryable()
            .OrderBy(x => x.Title)
            .ThenByDescending(x => x.Price)
            .ToList();

        return Ok(new { native, linq });
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