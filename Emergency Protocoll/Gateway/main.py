from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

from Clients.user_client import create_user, delete_user
from Clients.consumtion_client import get_consumption
from Clients.production_client import get_production



app = FastAPI(title="Python gRPC Gateway")

# ---------------- USERS ----------------

class UserCreate(BaseModel):
    username: str
    role: str

@app.post("/api/users")
def create_user_endpoint(user: UserCreate):
    try:
        response = create_user(user.username, user.role)
        return {"id": response.id}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/api/users/{user_id}")
def delete_user_endpoint(user_id: str):
    response = delete_user(user_id)
    if not response.success:
        raise HTTPException(status_code=404, detail="User not found")
    return {"success": True}

# ---------------- CONSUMPTION ----------------

@app.get("/api/consumption/{device_id}")
def get_consumption_endpoint(device_id: str):
    try:
        response = get_consumption(device_id)
        return {
            "device_id": response.device_id,
            "value": response.value,
            "timestamp": response.timestamp
        }
    except Exception:
        raise HTTPException(status_code=404, detail="Consumption not found")

# ---------------- PRODUCTION ----------------

@app.get("/api/production/{source_id}")
def get_production_endpoint(source_id: str):
    try:
        response = get_production(source_id)
        return {
            "source_id": response.source_id,
            "value": response.value,
            "timestamp": response.timestamp
        }
    except Exception:
        raise HTTPException(status_code=404, detail="Production not found")
