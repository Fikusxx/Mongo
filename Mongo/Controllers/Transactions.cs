using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("transactions")]
public sealed class Transactions : ControllerBase
{
    private readonly IMongoCollection<Game> db;
    private static readonly Guid Id = Guid.Parse("82157646-b752-4899-904d-562dfd02f20c");

    public Transactions()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }
    
    /// <summary>
    /// standalone deployment do not support transaction
    /// https://www.mongodb.com/community/forums/t/getting-error-mongodb-transactions-with-c-and-the-net-framework/11359/4
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("create-in-transaction")]
    public async Task<IActionResult> CreateInTransaction()
    {
        using var session = await db.Database.Client.StartSessionAsync();
        
        try
        {
            session.StartTransaction();
            
            await db.InsertOneAsync(session, new Game { Id = Id, Title = "Ori #1" });
            await db.InsertOneAsync(session, new Game { Id = Id, Title = "Ori #2" });

            await session.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        } 

        return Ok();
    }
}