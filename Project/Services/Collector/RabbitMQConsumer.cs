
using MongoDB.Bson;
using MongoDB.Driver;
using PvMeteringData;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace RabbitMQConsumer
{
    public class MessageData
    {
        public string PlantId { get; set; }
        public DateTime Timestamp { get; set; }
        public double PowerKw { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            var mongoSettings = config.GetSection("MongoDB").Get<MongoClientSettings>();
            string ConnectionString = "mongodb://127.0.0.1:27017";
            string DatabaseName = "pv_simulation";
            string collectionName = "metering";

            var mongoClient = new MongoClient(ConnectionString);
            IMongoDatabase mongodb = mongoClient.GetDatabase(DatabaseName);
            var mongoCollection = mongodb.GetCollection<BsonDocument>(collectionName);

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "user",
                Password = "userpassword"
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

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var mongoCollection = mongodb.GetCollection<PvMetering>("metering");
                var data = JsonSerializer.Deserialize<PvMetering>(json);
                await mongoCollection.InsertOneAsync(data);

                Console.WriteLine($"[SAVED] {data.PlantId} {data.Timestamp}");

                channel.BasicAck(ea.DeliveryTag, false);
            };

            channel.BasicConsume(
                queue: "pv_iot_metering",
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("Collector läuft...");
            Console.ReadLine();
        }
    }
}
