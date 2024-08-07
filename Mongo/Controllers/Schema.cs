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

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> Create()
    {
	    await ChangeSchemaValidation();

	    return Ok();
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

    /// <summary>
    /// https://www.devxperiences.com/pzwp1/2022/03/11/mongodb-schema-validation-rules/
    /// </summary>
    private async Task<BsonDocument> ChangeSchemaValidation()
    {
        const string jsonSchema = """
                                  { collMod: "Games",
                                  		validator: {
                                  			$jsonSchema: {
                                  				bsonType: "object",
                                  				required: [ "Title", "Price" ],
                                  				properties: {
                                  				Title: {
                                  					bsonType: "string",
                                  					minLength: 1,
                                  					maxLength: 20,
                                  					description: "must be a string of at least 1 to 20 characters and is required"
                                  				},
                                  				Price: {
                                  					bsonType: "int",
                                  					minimum: 1,
                                  					maximum: 1000
                                  					description: "must be from 1$ to 1000$"
                                  				}
                                  				}
                                  			}
                                  		},
                                  		validationAction: "error",
                                  		validationLevel: "strict",
                                  }
                                  """;
        
        var command = new JsonCommand<BsonDocument>(jsonSchema);
        var result = await db.Database.RunCommandAsync(command);

        return result;
    }
}