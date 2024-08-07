


using Mongo.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

#region Mongo DB

// BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
//
// BsonClassMap.RegisterClassMap<Game>(map =>
// {
//     // maps all properties automatically, only required along with other mappings
//     map.AutoMap();
//
//     // properties that are not mapped will be null assigned
//     map.MapIdProperty(x => x.Id);
//     map.MapMember(x => x.Title);
//     map.MapMember(x => x.Price);
//
//     map.SetIgnoreExtraElements(true);
// });


#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();