using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Controllers;

/// <summary>
/// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/time-series/
/// </summary>
[ApiController]
[Route("time-series")]
public sealed class TimeSeries : ControllerBase
{
    private readonly IMongoCollection<Sensor> db;

    public TimeSeries()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Sensor>(name: "Sensors");
    }

    [HttpPost]
    [Route("create-db")]
    public async Task<IActionResult> CreateDb()
    {
        var tsOptions = new TimeSeriesOptions(timeField: "timestamp");
        var collOptions = new CreateCollectionOptions { TimeSeriesOptions = tsOptions };
        
        var client = new MongoClient(DBConstants.ConnectionString);
        await client.GetDatabase("Personal").CreateCollectionAsync("Sensors", collOptions);

        return Ok();
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        await db.InsertManyAsync([
            new Sensor
            {
                Metadata = new BsonDocument().Add(name: "sensorId", value: 1).Add(name: "type", value: "timestamp"),
                Temperature = 10
            },
            new Sensor
            {
                Metadata = new BsonDocument().Add(name: "sensorId", value: 2).Add(name: "type", value: "timestamp"),
                Temperature = 20
            },
            new Sensor
            {
                Metadata = new BsonDocument().Add(name: "sensorId", value: 3).Add(name: "type", value: "timestamp"),
                Temperature = 30
            },
        ]);

        return Ok();
    }

    public class Sensor
    {
        public BsonDocument Metadata { get; set; } = new();
        public BsonDocument Timestamp { get; set; } = new BsonDocument().Add(name: "timestamp", value: DateTime.Now.ToBsonDocument());
        public int Temperature { get; set; }
    }
}