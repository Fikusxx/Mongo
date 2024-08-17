using Microsoft.AspNetCore.Mvc;
using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Controllers;

/// <summary>
/// https://www.mongodb.com/docs/v5.0/core/schema-validation/
/// https://www.mongodb.com/docs/manual/core/schema-validation/specify-json-schema/json-schema-tips/
/// https://www.mongodb.com/docs/manual/reference/bson-types/
/// </summary>
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
    [Route("apply")]
    public async Task<IActionResult> Apply()
    {
	    var result = await ChangeSchemaValidation();

	    return Ok(result);
    }
    
    private void CreateCollection()
    {
        db.Database.CreateCollection(name: "Games", new CreateCollectionOptions<Game>()
        {
            Validator = new FilterDefinitionBuilder<Game>().JsonSchema(BsonDocument.Parse("")),
            ValidationAction = DocumentValidationAction.Error,
            ValidationLevel = DocumentValidationLevel.Strict,
            Capped = true, // default false
            MaxSize = 100, // default long.MaxValue
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