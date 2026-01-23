import grpc
from proto import user_pb2
from proto import user_pb2_grpc

channel = grpc.insecure_channel("localhost:50051")
client = user_pb2_grpc.UserServiceStub(channel)

def create_user(username: str, role: str):
    return client.CreateUser(
        user_pb2.CreateUserRequest(
            username=username,
            role=role
        )
    )

def delete_user(user_id: str):
    return client.DeleteUser(
        user_pb2.DeleteUserRequest(id=user_id)
    )
