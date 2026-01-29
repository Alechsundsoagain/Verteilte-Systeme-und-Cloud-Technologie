using Metering;
using RabbitMQ.Client;
using PvMeteringData;
using System.Text;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();

var random = new Random();

Console.WriteLine("PV IoT Worker gestartet...");

while (true)
{
    var data = new PvMetering
    {
        PlantId = "PV-001",
        Timestamp = DateTime.UtcNow,
        PowerKw = Math.Round(random.NextDouble() * 10, 2), // 0–10 kW
        Voltage = Math.Round(380 + random.NextDouble() * 40, 2),
        Current = Math.Round(random.NextDouble() * 25, 2)
    };

    try
    {
        RabbitMQSender rabbitMQSender = new RabbitMQSender("localhost", "guest", "guest");

        // Serialize to JSON -> to RabbitMQ
        string json = JsonSerializer.Serialize(data);
        rabbitMQSender.Publish("simulation.update", json);
        Console.WriteLine($"Data sent to RabbitMQ: {json}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"RabbitMQ error: {ex.Message}");
    }

    Thread.Sleep(2000);
}

