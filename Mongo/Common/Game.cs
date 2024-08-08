using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Common;

public sealed class Game
{
    // mapped to _id by default
    public Guid Id { get; init; }
    
    [BsonElement("Title")] // has an effect, not here though, cuz names match
    public string? Title { get; init; }
    
    public int Price { get; init; } = 69;
    
    public DateOnly DateOnly { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}