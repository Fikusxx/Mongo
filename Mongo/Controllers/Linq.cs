using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Mongo.Controllers;

/// <summary>
/// Supported LINQ
/// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/linq/
/// </summary>
[ApiController]
[Route("linq")]
public sealed class Linq : ControllerBase
{
    private readonly IMongoCollection<Game> gamesDb;
    private readonly IMongoCollection<Catalog> catalogDb;

    public Linq()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.gamesDb = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
        this.catalogDb = client.GetDatabase("Personal").GetCollection<Catalog>(name: "Catalog");
    }

    [HttpGet]
    [Route("join-one-to-many")]
    public async Task<IActionResult> JoinOneToMany()
    {
        List<Game> dummyGames =
        [
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 10 },
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 20 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 30 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 40 },
        ];

        List<Catalog> dummyCatalogues =
        [
            new Catalog { Id = Guid.NewGuid(), Name = "Ori" },
            new Catalog { Id = Guid.NewGuid(), Name = "HK" },
        ];

        await gamesDb.InsertManyAsync(dummyGames);
        await catalogDb.InsertManyAsync(dummyCatalogues);

        var gamesQuery = gamesDb.AsQueryable();
        var catalogQuery = catalogDb.AsQueryable();

        // 1 to many
        var result = await catalogQuery.GroupJoin(gamesQuery,
            catalog => catalog.Name,
            game => game.Title,
            (catalog, games) =>
                new
                {
                    Catalog = catalog.Name,
                    Games = games
                }).ToListAsync();

        // clean up
        await gamesDb.DeleteManyAsync(_ => true);
        await catalogDb.DeleteManyAsync(_ => true);

        return Ok(result);
    }
    
    [HttpGet]
    [Route("join-one-to-one")]
    public async Task<IActionResult> JoinOneToOne()
    {
        List<Game> dummyGames =
        [
            new Game { Id = Guid.NewGuid(), Title = "Ori", Price = 10 },
            new Game { Id = Guid.NewGuid(), Title = "HK", Price = 20 },
        ];

        List<Catalog> dummyCatalogues =
        [
            new Catalog { Id = Guid.NewGuid(), Name = "Ori" },
            new Catalog { Id = Guid.NewGuid(), Name = "HK" },
        ];

        await gamesDb.InsertManyAsync(dummyGames);
        await catalogDb.InsertManyAsync(dummyCatalogues);

        var gamesQuery = gamesDb.AsQueryable();
        var catalogQuery = catalogDb.AsQueryable();

        // 1 to 1
        var result = await catalogQuery.Join(gamesQuery,
            catalog => catalog.Name,
            game => game.Title,
            (catalog, game) =>
                new
                {
                    Catalog = catalog,
                    Game = game
                }).ToListAsync();
        
        // clean up
        await gamesDb.DeleteManyAsync(_ => true);
        await catalogDb.DeleteManyAsync(_ => true);

        return Ok(result);
    }
}