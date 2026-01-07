# Starte Ubuntu
wsl

# Install k3d Windows (WSL2)
echo "Install k3d"
curl -s https://raw.githubusercontent.com/k3d-io/k3d/main/install.sh | bash

# Install helm
echo "Install Helm"
curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3
chmod 700 get_helm.sh
./get_helm.sh

# Erstellen eines k3s Clusters mit 3 Worker Nodes
echo "Create Cluster"
k3d cluster create energy-community \
 --servers 1 \
 --agents 3 \
 --port "8080:80@loadbalancer" \
 --port "8443:443@loadbalancer" \
 --volume "$(pwd)/data:/data@all" \
 --registry-create energy-registry:5000

# Verifizieren
echo "Verification"
kubectl get nodes
kubectl get namespaces

# Namespace erstellen
kubectl create namespace energy-system

# RabbitMQ installieren (Helm)
helm repo add bitnami https://charts.bitnami.com/bitnami
helm install rabbitmq bitnami/rabbitmq \
 --namespace energy-system \
 --set auth.username=admin \
 --set auth.password=secretpass

# MongoDB installieren
helm install mongodb bitnami/mongodb \
 --namespace energy-system \
 --set global.security.allowInsecureImages=true \
 --set image.repository=bitnamisecure/mongodb \
 --set image.tag=latest --set auth.enabled=true \
 --set auth.rootPassword=secretpass

# RabbitMQ Management UI verfügbar machen (http://localhost:15672)
kubectl port-forward -n energy-system svc/rabbitmq 15672:15672 &

# RabbitMQ AMQP Port für lokale Entwicklung
kubectl port-forward -n energy-system svc/rabbitmq 5672:5672 &

# MongoDB für lokale Entwicklung
kubectl port-forward -n energy-system svc/mongodb 27017:27017 &
 
# Credentials:
# RabbitMQ: admin/secretpass
# MongoDB: root/secretpass

# Problem: install rabbit&mongo; Solution: login to docker account in wsl
# Problem: Port forwarding; Solution: do it again in 2h
# Problem: Lens doesnt find Kubernetes; Solution: cp ~/.kube/config /mnt/c/Users/"Marius Moser"/.kube/k3d-config
# Problem: Cant connect to Mongo DB; Solution: use root and secretpass to access DB