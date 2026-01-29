import pika
import random
import time
import json
import sys

# ---------------- CONFIG ----------------

RABBITMQ_HOST = "localhost"
QUEUE_NAME = "iot_metering"

SEND_INTERVAL_SECONDS = 2  # how often values are sent

# ---------------- MODES ----------------
# mode 1:  0  ->  1
# mode 2: -1  ->  1
# mode 3: -1  ->  0

def generate_value(mode: int) -> float:
    #KWH
    if mode == 1:
        return random.uniform(0, 1000)
    elif mode == 2:
        return random.uniform(-1000, 1000)
    elif mode == 3:
        return random.uniform(-1000, 0)
    else:
        raise ValueError("Mode must be 1 (Consumer), 2 (Prosumer), or 3 (Producer)")

# ---------------- RABBITMQ ----------------

def create_channel():
    connection = pika.BlockingConnection(
        pika.ConnectionParameters(host=RABBITMQ_HOST)
    )
    channel = connection.channel()
    channel.queue_declare(queue=QUEUE_NAME, durable=True)
    return connection, channel

# ---------------- MAIN LOOP ----------------

def main(mode: int):
    connection, channel = create_channel()

    print(f"[IOT] Metering started in mode {mode}")

    try:
        while True:
            value = generate_value(mode)

            message = {
                "mode": mode,
                "value": round(value, 4),
                "timestamp": int(time.time())
            }

            channel.basic_publish(
                exchange="",
                routing_key=QUEUE_NAME,
                body=json.dumps(message),
                properties=pika.BasicProperties(
                    delivery_mode=2  # persistent
                )
            )

            print(f"[IOT] Sent → {message}")

            time.sleep(SEND_INTERVAL_SECONDS)

    except KeyboardInterrupt:
        print("\n[IOT] Stopped")

    finally:
        connection.close()

# ---------------- ENTRYPOINT ----------------

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python iot_metering.py <mode>")
        print("Modes:")
        print("  1 -> values from 0 to 1")
        print("  2 -> values from -1 to 1")
        print("  3 -> values from -1 to 0")
        sys.exit(1)

    selected_mode = int(sys.argv[1])
    main(selected_mode)
