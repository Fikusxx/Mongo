using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace Mongo.Controllers;

/// <summary>
/// https://www.mongodb.com/docs/manual/geospatial-queries/
/// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/geo/
/// https://www.mongodb.com/docs/manual/reference/geojson/
/// https://www.google.com/maps
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
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/geo/#query-by-proximity
    /// условно ближайшие рестораны к человеку
    /// </summary>
    [HttpGet]
    [Route("proximity")]
    public async Task<IActionResult> Proximity()
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

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/geo/#query-by-polygon
    /// можно определить находятся ли какие то точки внутри данного полигона
    /// </summary>
    [HttpGet]
    [Route("polygon")]
    public async Task<IActionResult> Polygon()
    {
        await db.InsertManyAsync([
            new Store { Coordinates = [56.301658, 43.957012] }, // мой дом
            new Store { Coordinates = [56.302346, 43.929337] }, // мама
            new Store { Coordinates = [56.303067, 43.936702] }, // бабушка
        ]);

        // НН
        // first and last position must be the same
        var polygon = GeoJson.Polygon
        (
            GeoJson.Position(56.386269, 43.786857),
            GeoJson.Position(56.193896, 43.780808),
            GeoJson.Position(56.191204, 44.083878),
            GeoJson.Position(56.317891, 44.074804),
            GeoJson.Position(56.386269, 43.786857)
        );

        var filter = Builders<Store>.Filter.GeoWithin(x => x.Coordinates, geometry: polygon);
        var result = await db.Find(filter).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }

    /// <summary>
    /// Coordinates в данном случае должен быть условно Type = "Polygon", Coordinates = [ [ [1, 1], [2,2], [3,3], [1,1] ], ie double[][][]
    /// Можно определить находится ли какая то точка (юзер) внутри некого полигона (город)
    /// </summary>
    [HttpGet]
    [Route("intersects")]
    public async Task<IActionResult> Intersects()
    {
        var refPoint = GeoJson.Point(GeoJson.Position(-73.98456, 40.7612));
        var filter = Builders<Store>.Filter.GeoIntersects(x => x.Coordinates, refPoint);
        var result = await db.Find(filter).ToListAsync();

        // clean up
        await db.DeleteManyAsync(_ => true);

        return Ok(result);
    }

    /// <summary>
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/indexes/#geospatial-indexes
    /// </summary>
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        var index = Builders<Store>.IndexKeys.Geo2DSphere(x => x);
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
}