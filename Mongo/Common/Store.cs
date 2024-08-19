namespace Mongo.Common;

public class Store
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Type { get; init; } = "Point";
    public required double[] Coordinates { get; init; }
}