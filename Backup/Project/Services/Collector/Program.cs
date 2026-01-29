using Collector;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

////////////////////////
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;

// MongoDB
var mongoClient = new MongoClient("mongodb://localhost:27017");
var database = mongoClient.GetDatabase("pv_simulation");
var collection = database.GetCollection<PvMeteringData>("metering");

// RabbitMQ
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    DispatchConsumersAsync = true
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "pv_iot_metering",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

// Fair Dispatch
channel.BasicQos(0, 1, false);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.Received += async (sender, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        var data = JsonSerializer.Deserialize<PvMeteringData>(json);

        await collection.InsertOneAsync(data);

        Console.WriteLine($"[SAVED] {data.PlantId} {data.Timestamp}");

        channel.BasicAck(ea.DeliveryTag, false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
        // Nachricht bleibt in der Queue
    }
};

channel.BasicConsume(
    queue: "pv_iot_metering",
    autoAck: false,
    consumer: consumer
);

Console.WriteLine("Collector Worker läuft...");
Console.ReadLine();
