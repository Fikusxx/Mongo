using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Mongo.Controllers;


/// <summary>
/// https://www.mongodb.com/docs/manual/geospatial-queries/
/// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/geo/
/// </summary>
[ApiController]
[Route("geo")]
public sealed class Geo : ControllerBase
{
    private readonly IMongoCollection<Store> db;

    public Geo()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Store>(name: "Stores");
    }

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/indexes/#geospatial-indexes
    /// </summary>
    [HttpGet]
    [Route("nearest")]
    public async Task<IActionResult> GetNearest()
    {
        await db.InsertManyAsync([
            new Store { Coordinates = [-73.98, 40.76] },
            new Store { Coordinates = [-73.98456, 38.7612] },
            new Store { Coordinates = [-73.98456, 39.7612] }, // distance 111.19 km
        ]);

        var refPoint = GeoJson.Point(GeoJson.Position(-73.98456, 40.7612));

        var filter =
            Builders<Store>.Filter.Near(x => x.Coordinates, point: refPoint, maxDistance: 112000, minDistance: 0);
        var result = await db.Find(filter).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateGeoIndex()
    {
        var index = Builders<Store>.IndexKeys.Geo2DSphere(x => x.Coordinates);
        var indexOptions = new CreateIndexOptions { Name = "Geo_777" };
        var indexModel = new CreateIndexModel<Store>(index, indexOptions);
        var indexName = await db.Indexes.CreateOneAsync(indexModel);

        return Ok(indexName);
    }
    
    [HttpDelete]
    [Route("purge")]
    public async Task<IActionResult> Purge()
    {
        await db.DeleteManyAsync(_ => true);

        return Ok();
    }

    public class Store
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = "Point";
        public double[] Coordinates { get; set; }
    }
}