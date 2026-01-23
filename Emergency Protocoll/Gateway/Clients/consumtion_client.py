import grpc
from proto import consumption_pb2
from proto import consumption_pb2_grpc

channel = grpc.insecure_channel("localhost:50052")
client = consumption_pb2_grpc.ConsumptionServiceStub(channel)

def get_consumption(device_id: str):
    return client.GetConsumption(
        consumption_pb2.GetConsumptionRequest(
            device_id=device_id
        )
    )
