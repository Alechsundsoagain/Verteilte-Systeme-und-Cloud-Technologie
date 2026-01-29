using Metering;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

///////////////////////
//using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

// Queue deklarieren
channel.QueueDeclare(
    queue: "pv_iot_metering",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var random = new Random();

Console.WriteLine("PV IoT Worker gestartet...");

while (true)
{
    var data = new PvMeteringData
    {
        PlantId = "PV-001",
        Timestamp = DateTime.UtcNow,
        PowerKw = Math.Round(random.NextDouble() * 10, 2), // 0–10 kW
        Voltage = Math.Round(380 + random.NextDouble() * 40, 2),
        Current = Math.Round(random.NextDouble() * 25, 2)
    };

    var json = JsonSerializer.Serialize(data);
    var body = Encoding.UTF8.GetBytes(json);

    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;

    channel.BasicPublish(
        exchange: "",
        routingKey: "pv_iot_metering",
        basicProperties: properties,
        body: body
    );

    Console.WriteLine($"[SENT] {json}");

    Thread.Sleep(2000); // alle 2 Sekunden
}