using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mongo;

/// <summary>
/// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/class-mapping/
/// </summary>
public static class WhoAmI
{
    public static IServiceCollection AddMongoSettings(this IServiceCollection services)
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        // BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        
        
        BsonClassMap.RegisterClassMap<Game>(map =>
        {
            // default behavior if none specified
            // map.AutoMap();

            // by default uses default ctor
            // map.MapConstructor(new ConstructorInfo(), args) // or [BsonConstructor]
            // map.MapCreator(x => new Game(args...)); // specify ctor explicitly

            // properties that are not mapped will NOT exist in the db, thus throw on retrieval from db
            map.MapIdProperty(x => x.Id);

            map.MapMember(x => x.Title)
                .SetIsRequired(true) // doesnt work?
                .SetElementName("Title") // overrides [BsonElement("Title")]
                .SetDefaultValue("Ori"); // doesnt work?

            map.MapMember(x => x.Price);
            // .SetShouldSerializeMethod(x => ((Game)x).Price != 69); ignores on SERIALIZE to BSON

            map.MapMember(x => x.DateOnly);
            //  .SetSerializer(new DateTimeSerializer(dateOnly: true)); if it's DateTime / DateTimeOffset takes only DateOnly part
            //  .SetSerializer(new DateTimeSerializer(DateTimeKind.Local));

            // TRUE : ignores properties IN BSON that do NOT have a match for C# class property
            // FALSE: throws if BSON do NOT have a match for C# class property AND that property is either 1) mapped explicitly 2) automap is used
            map.SetIgnoreExtraElements(true);
        });

        return services;
    }

    /// <summary>
    /// // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/logging/
    /// </summary>
    public static IServiceCollection AddMongoLogging(this IServiceCollection services)
    {
        // var categoriesConfiguration = new Dictionary<string, string>
        // {
        //     { "LogLevel:Default", "Debug" },
        //     { "LogLevel:MongoDB.SDAM", "Error" }
        // };
        // var config = new ConfigurationBuilder()
        //     .AddInMemoryCollection(categoriesConfiguration)
        //     .Build();
        // var loggerFactory = LoggerFactory.Create(b =>
        // {
        //     b.AddConfiguration(config);
        //     b.AddSimpleConsole();
        // });
        //
        // var settings = MongoClientSettings.FromConnectionString(DBConstants.ConnectionString);
        // settings.LoggingSettings = new LoggingSettings(loggerFactory);
        // var client = new MongoClient(settings);
        
        return services;
    }
}