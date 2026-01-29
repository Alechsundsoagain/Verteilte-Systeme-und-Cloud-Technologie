#!/bin/bash

# create-team-kubeconfig.sh
# Generiert eine Team-spezifische kubeconfig mit eingeschränkten Rechten
# Usage: ./create-team-kubeconfig.sh <team-name> <service-account> <namespace>

set -e

if [ "$#" -ne 3 ]; then
    echo "Usage: $0 <team-name> <service-account> <namespace>"
    echo "Example: $0 team-1 team-1-deployer team-1"
    exit 1
fi

TEAM_NAME=$1
SERVICE_ACCOUNT=$2
NAMESPACE=$3
KUBECONFIG_FILE="${TEAM_NAME}-kubeconfig.yaml"

echo "Generating kubeconfig for team: ${TEAM_NAME}"
echo "Service Account: ${SERVICE_ACCOUNT}"
echo "Namespace: ${NAMESPACE}"

# 1. Get the service account token
echo "Fetching service account token..."
TOKEN=$(kubectl create token ${SERVICE_ACCOUNT} -n ${NAMESPACE} --duration=8760h)

# 2. Get cluster CA certificate from current context
echo "Fetching cluster CA certificate..."
CA_DATA=$(kubectl config view --raw -o jsonpath='{.clusters[?(@.name=="'$(kubectl config current-context | xargs kubectl config view -o jsonpath='{.contexts[?(@.name=="'$(kubectl config current-context)'")].context.cluster}' --raw)'")].cluster.certificate-authority-data}')

# Fallback: Direkt vom aktuellen Cluster
if [ -z "$CA_DATA" ]; then
    CURRENT_CLUSTER=$(kubectl config view --raw -o jsonpath='{.contexts[?(@.name=="'$(kubectl config current-context)'")].context.cluster}')
    CA_DATA=$(kubectl config view --raw -o jsonpath='{.clusters[?(@.name=="'$CURRENT_CLUSTER'")].cluster.certificate-authority-data}')
fi

# 3. Get cluster server URL
echo "Fetching cluster server URL..."
CURRENT_CLUSTER=$(kubectl config view --raw -o jsonpath='{.contexts[?(@.name=="'$(kubectl config current-context)'")].context.cluster}')
CLUSTER_SERVER=$(kubectl config view --raw -o jsonpath='{.clusters[?(@.name=="'$CURRENT_CLUSTER'")].cluster.server}')

# 4. Generate kubeconfig
echo "Generating kubeconfig file: ${KUBECONFIG_FILE}"
cat > ${KUBECONFIG_FILE} <<EOF
apiVersion: v1
kind: Config
clusters:
- name: ${TEAM_NAME}-cluster
  cluster:
    certificate-authority-data: ${CA_DATA}
    server: ${CLUSTER_SERVER}
contexts:
- name: ${TEAM_NAME}-context
  context:
    cluster: ${TEAM_NAME}-cluster
    namespace: ${NAMESPACE}
    user: ${TEAM_NAME}-user
current-context: ${TEAM_NAME}-context
users:
- name: ${TEAM_NAME}-user
  user:
    token: ${TOKEN}
EOF

echo ""
echo "✅ Kubeconfig generated successfully: ${KUBECONFIG_FILE}"
echo ""
echo "To use this kubeconfig:"
echo "  export KUBECONFIG=$(pwd)/${KUBECONFIG_FILE}"
echo "  kubectl get pods"
echo ""
echo "To test access:"
echo "  kubectl --kubeconfig=${KUBECONFIG_FILE} get pods -n ${NAMESPACE}"
echo ""
