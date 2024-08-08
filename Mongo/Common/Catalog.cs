namespace Mongo.Common;

public sealed class Catalog
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}