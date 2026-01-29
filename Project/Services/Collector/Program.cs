using Collector;
using MongoDB.Driver;
using PvMeteringData;

var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("pv_simulation");
var collection = database.GetCollection<PvMetering>("metering");

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

