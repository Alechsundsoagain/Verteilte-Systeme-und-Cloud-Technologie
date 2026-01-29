import pika
from pymongo import MongoClient
from datetime import datetime

QUEUE_NAME = "iot_metering"

def main():
    # ----- MongoDB -----
    mongo_client = MongoClient("mongodb://localhost:27017")
    db = mongo_client["iot_data"]
    collection = db["metering_values"]

    # ----- RabbitMQ -----
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host="localhost")
    )
    channel = connection.channel()

    channel.queue_declare(queue=QUEUE_NAME)

    print("Collector started. Waiting for messages...")

    def callback(ch, method, properties, body):
        try:
            value = float(body.decode())
            document = {
                "value": value,
                "timestamp": datetime.utcnow(),
                "source": "iot_metering"
            }
            collection.insert_one(document)
            print(f"Stored value: {value}")
        except ValueError:
            print(f"Invalid message received: {body}")

    channel.basic_consume(
        queue=QUEUE_NAME,
        on_message_callback=callback,
        auto_ack=True
    )

    try:
        channel.start_consuming()
    except KeyboardInterrupt:
        print("Stopping collector...")
    finally:
        connection.close()
        mongo_client.close()

if __name__ == "__main__":
    main()
