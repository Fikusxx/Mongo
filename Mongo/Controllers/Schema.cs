using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Controllers;

[ApiController]
[Route("schema")]
public sealed class Schema : ControllerBase
{
    private readonly IMongoCollection<Game> db;

    public Schema()
    {
        var client = new MongoClient(DBConstants.ConnectionString);
        this.db = client.GetDatabase("Personal").GetCollection<Game>(name: "Games");
    }
    
    private void CreateCollection()
    {
        db.Database.CreateCollection(name: "Games", new CreateCollectionOptions<Game>()
        {
            Validator = new FilterDefinitionBuilder<Game>().JsonSchema(BsonDocument.Parse("")),
            ValidationAction = DocumentValidationAction.Error,
            ValidationLevel = DocumentValidationLevel.Strict
        });
    }

    private async Task ChangeSchemaValidation()
    {
        var jsonSchema = """
                         { collMod: "Games",
                         		validator: {
                         			$jsonSchema: {
                         				bsonType: "object",
                         				required: [ "Title", "Price" ],
                         				properties: {
                         				Title: {
                         					bsonType: "string",
                         					minLength: 3,
                         					description: "must be a string of at least 3 characters and is required"
                         				},
                         				Price: {
                         					bsonType: "int???",
                         					description: "must be a ...."
                         				}
                         				}
                         			}
                         		},
                         		validationAction: "error",
                         		validationLevel: "strict",
                         		}
                         """;
        
        var command = new JsonCommand<BsonDocument>(jsonSchema);
        await db.Database.RunCommandAsync(command);
    }
}