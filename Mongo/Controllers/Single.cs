using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("single")]
public sealed class Single : ControllerBase
{
    private readonly IMongoCollection<Game> db;
    private static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");

    public Single()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }

    [HttpGet]
    [Route("find-single")]
    public async Task<IActionResult> Get()
    {
        var filter = Builders<Game>.Filter.Eq(x => x.Id, Id);
        var resultWithFilter = await db.Find(filter).FirstOrDefaultAsync();

        var resultWithLambda = await db.Find(x => x.Id == Id).FirstOrDefaultAsync();

        return Ok(new { resultWithFilter, resultWithLambda });
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create([FromQuery] string? title, [FromQuery] int? price)
    {
        try
        {
            await db.InsertOneAsync(new Game { Id = Id, Title = title ?? "Ori", Price = price ?? 69 });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }

    [HttpPut]
    [Route("update")]
    public async Task<IActionResult> Update()
    {
        // update commands DO NOT create new resources by default, ie IsUpsert = false

        // 2 set commands modifying 1 field is okay
        // var update1 = Builders<Game>.Update.Set(x => x.Title, "Ori and Will of the Wisps #1");
        // var update2 = Builders<Game>.Update.Set(x => x.Title, "Ori and Will of the Wisps #2");
        // var finalUpdate = Builders<Game>.Update.Combine(update1, update2);
        // await db.FindOneAndUpdateAsync(x => x.Id == Id, finalUpdate);

        // 2 different commands (Set & Inc) modifying 1 field will throw
        // var update1 = Builders<Game>.Update.Set(x => x.Price, 70);
        // var update2 = Builders<Game>.Update.Inc(x => x.Price, 10);
        // var finalUpdate = Builders<Game>.Update.Combine(update1, update2);
        // await db.FindOneAndUpdateAsync(x => x.Id == Id, finalUpdate);

        // will only update Price prop to 60 if it's higher than 155.
        // same goes for Max();
        // var update = Builders<Game>.Update.Min(x => x.Price, 60);
        // await db.FindOneAndUpdateAsync(x => x.Id == Id, update);

        // Builders<Game>.Update.Inc(x => x.Price, 5); - increment by 5
        // Builders<Game>.Update.Inc(x => x.Price, -5); - decrement by 5

        // Builders<Game>.Update.Unset(x => x.Item);
        // deletes field completely from a document

        // Builders<Game>.Update.Rename(x => x.Item, "NewItem");
        // renames field from a document

        // Builders<Game>.Update.AddToSet(x => x.Genres, "Adventure");
        // add an item to an array only if it doesnt already exist

        // Builders<Game>.Update.Push(x => x.Genres, "New value");
        // add an item to an array even if it already exists

        // removes a value(s)
        // Builders<Game>.Update.Pull(x => x.Genres, "Adventure");
        // Builders<Game>.Update.PullAll(x => x.Genres, ["Action", "RPG"]);
        // Builders<Game>.Update.PullFilter(x => x.Genres, y => y == "value");

        var update = Builders<Game>.Update.Set(x => x.Title, "Ori and Will of the Wisps");
        var oldDocument = await db.FindOneAndUpdateAsync(x => x.Id == Id, update);
        var updateResult = await db.UpdateOneAsync(x => x.Id == Id, update);

        // var updateResult = await db.UpdateOneAsync(x => x.Id == Id, update, new UpdateOptions { IsUpsert = true });

        // updateResult
        // {
        // "isAcknowledged": true,
        //  "isModifiedCountAvailable": true,
        //  "matchedCount": 0,
        //  "modifiedCount": 0,
        //  "upsertedId": null
        // }

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(new { oldDocument, updateResult });
    }

    [HttpPut]
    [Route("replace")]
    public async Task<IActionResult> Replace()
    {
        // replace commands DO NOT create new resources by default

        var filter = Builders<Game>.Filter.Eq(x => x.Id, Id);
        var game = await db.Find(filter).FirstOrDefaultAsync();
        var newGame = new Game
        {
            Id = game.Id,
            Title = "Hollow Knight",
            Price = 123
        };

        var replaceResult = await db.ReplaceOneAsync(filter, newGame);
        var replacedDocument = await db.FindOneAndReplaceAsync(x => x.Id == Id, newGame);


        // will fail cause game already has an Id which is different from Guid.NewGuid()
        // db.ReplaceOne(x => x.Id == Guid.NewGuid(), game, new ReplaceOptions { IsUpsert = true });
        //
        // will not fail cause Id of the filter and the resource match, new Id = "Whatever Id"
        // var id = Guid.NewGuid();
        // game.Id = id;
        // db.ReplaceOne(x => x.Id == id, game, new ReplaceOptions { IsUpsert = true });
        //
        // will not fail nor insert a new document, because IsUpsert = false
        // var id2 = Guid.NewGuid();
        // game.Id = id2;
        // db.ReplaceOne(x => x.Id == id2, game, new ReplaceOptions { IsUpsert = false });


        // replaceResult
        // {
        // "isAcknowledged": true,
        //  "isModifiedCountAvailable": true,
        //  "matchedCount": 0,
        //  "modifiedCount": 0,
        //  "upsertedId": null
        // }

        return Ok(new { replaceResult, replacedDocument });
    }

    [HttpDelete]
    [Route("purge")]
    public async Task<IActionResult> Purge()
    {
        // await  db.Database.DropCollectionAsync("Games");
        var deleteManyResult = await db.DeleteManyAsync(_ => true);

        return Ok(deleteManyResult);
    }
}