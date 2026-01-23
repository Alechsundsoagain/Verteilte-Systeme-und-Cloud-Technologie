import grpc
from proto import production_pb2
from proto import production_pb2_grpc

channel = grpc.insecure_channel("localhost:50053")
client = production_pb2_grpc.ProductionServiceStub(channel)

def get_production(source_id: str):
    return client.GetProduction(
        production_pb2.GetProductionRequest(
            source_id=source_id
        )
    )
