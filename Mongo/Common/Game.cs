using MongoDB.Bson.Serialization.Attributes;

namespace Mongo.Common;

public sealed class Game
{
    // mapped to _id by default
    public Guid Id { get; set; }
    
    [BsonElement("Title")] // has an effect, not here though, cuz names match
    public string Title { get; set; }
    
    public int Price { get; set; } = 69;
}