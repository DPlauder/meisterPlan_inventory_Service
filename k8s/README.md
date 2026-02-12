# Kubernetes Deployment Manifests

Diese Verzeichnis enthält alle notwendigen Kubernetes-Manifests für das Deployment des Inventory Service.

## 📁 Dateien

- **namespace.yaml** - Erstellt einen dedizierten Namespace für den Service
- **configmap.yaml** - Konfigurationsvariablen (nicht-sensible Daten)
- **secrets.yaml** - Sensible Daten wie Passwörter (⚠️ Ändern Sie diese in Produktion!)
- **postgres-pvc.yaml** - Persistent Volume Claim für PostgreSQL-Daten
- **postgres-deployment.yaml** - PostgreSQL Datenbank Deployment
- **postgres-service.yaml** - PostgreSQL Service (ClusterIP)
- **inventory-deployment.yaml** - Inventory Service Deployment
- **inventory-service.yaml** - Inventory Service (LoadBalancer)
- **ingress.yaml** - Ingress für Domain-basierter Zugriff mit SSL

## 🚀 Schnellstart

### Alle Manifests deployen

```bash
kubectl apply -f k8s/
```

### Oder einzeln in dieser Reihenfolge:

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres-pvc.yaml
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/postgres-service.yaml
kubectl apply -f k8s/inventory-deployment.yaml
kubectl apply -f k8s/inventory-service.yaml
kubectl apply -f k8s/ingress.yaml
```

## 🔍 Status überprüfen

```bash
# Alle Ressourcen anzeigen
kubectl get all -n inventory-service

# Pods überwachen
kubectl get pods -n inventory-service -w

# Service-IP abrufen
kubectl get service inventory-service -n inventory-service

# Logs anzeigen
kubectl logs -n inventory-service deployment/inventory-service -f
```

## 🔐 Wichtige Hinweise

### Secrets ändern!

⚠️ **Die Datei `secrets.yaml` enthält ein Beispiel-Passwort!**

Für Produktion:

```bash
# Secrets NICHT aus YAML-Datei erstellen, sondern imperativ:
kubectl create secret generic inventory-secrets \
  --from-literal=POSTGRES_PASSWORD='your-super-secure-password' \
  -n inventory-service

# Dann secrets.yaml löschen oder nicht anwenden
```

### Domain konfigurieren

In `ingress.yaml`, ersetzen Sie:
```yaml
- host: inventory.example.com  # <- Ihre echte Domain
```

### Storage Class anpassen

In `postgres-pvc.yaml`, passen Sie die Storage Class an Ihren Provider an:

```yaml
storageClassName: standard  # Für GKE
# storageClassName: managed-premium  # Für AKS
# storageClassName: do-block-storage  # Für DigitalOcean
```

## 🌐 Zugriff auf den Service

### Ohne Ingress (LoadBalancer)

```bash
# Externe IP abrufen
kubectl get service inventory-service -n inventory-service

# Zugriff via: http://EXTERNAL-IP/api/inventory
```

### Mit Ingress (Domain)

1. Installieren Sie einen Ingress Controller (z.B. nginx-ingress)
2. Installieren Sie cert-manager für SSL
3. Passen Sie `ingress.yaml` an
4. Zugriff via: https://inventory.example.com/api/inventory

## 📊 Ressourcen

Der Service ist für moderate Lasten konfiguriert:

- **Inventory Service**: 2 Replicas, 256Mi-512Mi RAM, 0.25-0.5 CPU
- **PostgreSQL**: 1 Replica, 5Gi Storage

Passen Sie `inventory-deployment.yaml` an für andere Anforderungen.

## 🔄 Updates deployen

```bash
# Image-Tag ändern in inventory-deployment.yaml
# Dann:
kubectl apply -f k8s/inventory-deployment.yaml

# Oder Rolling Update durchführen:
kubectl set image deployment/inventory-service \
  inventory-service=ghcr.io/dplauder/meisterplan_inventory_service:v2.0 \
  -n inventory-service
```

## 🗑️ Cleanup

```bash
# Alle Ressourcen löschen
kubectl delete -f k8s/

# Oder nur den Namespace
kubectl delete namespace inventory-service
```

## 📚 Weitere Informationen

Siehe [DEPLOYMENT.md](../DEPLOYMENT.md) für vollständige Deployment-Anleitungen.
