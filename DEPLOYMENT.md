# 🌐 Deployment Guide - Inventory Service ins Internet

Vollständige Anleitung zum Deployment des Inventory Service Mikroservices, sodass dieser über das Internet erreichbar ist.

---

## 📑 Inhaltsverzeichnis

1. [Übersicht der Deployment-Optionen](#übersicht-der-deployment-optionen)
2. [VPS/Server Deployment (Docker Compose)](#vpsserver-deployment-docker-compose)
3. [Cloud-Plattform Deployment](#cloud-plattform-deployment)
   - [Azure Container Instances](#azure-container-instances)
   - [AWS ECS](#aws-ecs)
   - [Google Cloud Run](#google-cloud-run)
   - [DigitalOcean App Platform](#digitalocean-app-platform)
4. [Kubernetes Deployment](#kubernetes-deployment)
5. [Domain & SSL Setup](#domain--ssl-setup)
6. [Sicherheits-Best-Practices](#sicherheits-best-practices)
7. [Monitoring & Logs](#monitoring--logs)

---

## 🎯 Übersicht der Deployment-Optionen

### Einfachheit vs. Skalierbarkeit

| Option                    | Kosten   | Einfachheit | Skalierbarkeit | Empfohlen für             |
| ------------------------- | -------- | ----------- | -------------- | ------------------------- |
| **VPS (Docker Compose)**  | €5-20/M  | ⭐⭐⭐⭐⭐    | ⭐⭐            | Kleine bis mittlere Apps  |
| **Kubernetes**            | €20+/M   | ⭐⭐         | ⭐⭐⭐⭐⭐       | Enterprise Apps           |
| **Azure Container Inst.** | €10-50/M | ⭐⭐⭐⭐      | ⭐⭐⭐          | Schnelle Prototypen       |
| **AWS ECS**               | €10-50/M | ⭐⭐⭐       | ⭐⭐⭐⭐         | AWS-basierte Infrastruktur |
| **Google Cloud Run**      | €0-50/M  | ⭐⭐⭐⭐⭐    | ⭐⭐⭐⭐         | Serverless Anwendungen    |
| **DigitalOcean Apps**     | €5-30/M  | ⭐⭐⭐⭐⭐    | ⭐⭐⭐          | Entwickler-freundlich     |

---

## 🖥️ VPS/Server Deployment (Docker Compose)

### Schnellste und einfachste Methode für Produktion

### 1. VPS Provider wählen

Empfohlene Anbieter:
- **Hetzner Cloud** (Deutschland) - Ab €4.15/Monat
- **DigitalOcean** - Ab $6/Monat
- **Linode/Akamai** - Ab $5/Monat
- **Contabo** (Deutschland) - Ab €4.99/Monat

Mindestanforderungen:
- **2 GB RAM**
- **1 vCPU**
- **20 GB SSD**
- **Ubuntu 22.04 LTS**

### 2. Server einrichten

```bash
# 1. Mit SSH verbinden
ssh root@your-server-ip

# 2. System aktualisieren
apt update && apt upgrade -y

# 3. Docker installieren
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# 4. Docker Compose installieren
apt install docker-compose-plugin -y

# 5. Firewall konfigurieren
ufw allow 22    # SSH
ufw allow 80    # HTTP
ufw allow 443   # HTTPS
ufw enable
```

### 3. Repository klonen und deployen

```bash
# 1. Git installieren (falls nicht vorhanden)
apt install git -y

# 2. Repository klonen
cd /opt
git clone https://github.com/DPlauder/meisterPlan_inventory_Service.git
cd meisterPlan_inventory_Service

# 3. Umgebungsvariablen setzen
export DB_PASSWORD="your-secure-password-here"

# 4. Services mit Production-Konfiguration starten
docker compose -f docker-compose.production.yml up -d --build

# 5. Status prüfen
docker compose -f docker-compose.production.yml ps
docker compose -f docker-compose.production.yml logs -f
```

### 4. Service testen

```bash
# Von Server aus
curl http://localhost:8081/api/inventory

# Von außen (ersetzen Sie SERVER_IP)
curl http://SERVER_IP:8081/api/inventory
```

### 5. Automatischer Start beim Server-Neustart

```bash
# Systemd Service erstellen
cat > /etc/systemd/system/inventory-service.service << 'EOF'
[Unit]
Description=Inventory Service
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/meisterPlan_inventory_Service
ExecStart=/usr/bin/docker compose -f docker-compose.production.yml up -d
ExecStop=/usr/bin/docker compose -f docker-compose.production.yml down
Environment="DB_PASSWORD=your-secure-password-here"

[Install]
WantedBy=multi-user.target
EOF

# Service aktivieren
systemctl enable inventory-service
systemctl start inventory-service
systemctl status inventory-service
```

---

## ☁️ Cloud-Plattform Deployment

### Azure Container Instances

```bash
# 1. Azure CLI installieren
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# 2. Login
az login

# 3. Resource Group erstellen
az group create --name inventory-rg --location westeurope

# 4. Container Registry erstellen
az acr create --resource-group inventory-rg \
  --name inventoryregistry --sku Basic

# 5. Docker Image bauen und pushen
az acr build --registry inventoryregistry \
  --image inventory-service:latest .

# 6. PostgreSQL Datenbank erstellen
az postgres flexible-server create \
  --resource-group inventory-rg \
  --name inventory-db \
  --admin-user postgres \
  --admin-password 'SecurePassword123!' \
  --sku-name Standard_B1ms

# 7. Container Instance deployen
az container create \
  --resource-group inventory-rg \
  --name inventory-service \
  --image inventoryregistry.azurecr.io/inventory-service:latest \
  --dns-name-label inventory-service-unique \
  --ports 8080 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Host=inventory-db.postgres.database.azure.com;Database=inventorydb;Username=postgres;Password=SecurePassword123!;SslMode=Require"
```

**Zugriff**: `http://inventory-service-unique.westeurope.azurecontainer.io:8080/api/inventory`

### AWS ECS (Elastic Container Service)

```bash
# 1. AWS CLI installieren
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install

# 2. Credentials konfigurieren
aws configure

# 3. ECR Repository erstellen
aws ecr create-repository --repository-name inventory-service

# 4. Docker Image bauen und pushen
aws ecr get-login-password --region eu-central-1 | \
  docker login --username AWS --password-stdin ACCOUNT_ID.dkr.ecr.eu-central-1.amazonaws.com

docker build -t inventory-service .
docker tag inventory-service:latest ACCOUNT_ID.dkr.ecr.eu-central-1.amazonaws.com/inventory-service:latest
docker push ACCOUNT_ID.dkr.ecr.eu-central-1.amazonaws.com/inventory-service:latest

# 5. RDS PostgreSQL erstellen (via AWS Console oder CLI)
# 6. ECS Cluster und Task Definition erstellen (via AWS Console)
```

### Google Cloud Run

```bash
# 1. gcloud CLI installieren
curl https://sdk.cloud.google.com | bash
exec -l $SHELL
gcloud init

# 2. Projekt erstellen
gcloud projects create inventory-service-project
gcloud config set project inventory-service-project

# 3. Cloud SQL PostgreSQL Instanz erstellen
gcloud sql instances create inventory-db \
  --database-version=POSTGRES_16 \
  --tier=db-f1-micro \
  --region=europe-west1

# 4. Datenbank erstellen
gcloud sql databases create inventorydb --instance=inventory-db

# 5. Cloud Build aktivieren und Image bauen
gcloud builds submit --tag gcr.io/inventory-service-project/inventory-service

# 6. Cloud Run Service deployen
gcloud run deploy inventory-service \
  --image gcr.io/inventory-service-project/inventory-service \
  --platform managed \
  --region europe-west1 \
  --allow-unauthenticated \
  --add-cloudsql-instances inventory-service-project:europe-west1:inventory-db \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__DefaultConnection=Host=/cloudsql/inventory-service-project:europe-west1:inventory-db;Database=inventorydb;Username=postgres;Password=YOUR_PASSWORD"
```

**Zugriff**: Automatisch generierte URL wie `https://inventory-service-xxx.run.app`

### DigitalOcean App Platform

```bash
# 1. doctl CLI installieren
cd /tmp
wget https://github.com/digitalocean/doctl/releases/download/v1.100.0/doctl-1.100.0-linux-amd64.tar.gz
tar xf doctl-1.100.0-linux-amd64.tar.gz
sudo mv doctl /usr/local/bin

# 2. Authentifizieren
doctl auth init

# 3. PostgreSQL Datenbank erstellen
doctl databases create inventory-db --engine pg --region fra1 --size db-s-1vcpu-1gb

# 4. App Spec YAML erstellen
cat > .do/app.yaml << 'EOF'
name: inventory-service
region: fra
services:
  - name: web
    github:
      repo: DPlauder/meisterPlan_inventory_Service
      branch: main
      deploy_on_push: true
    build_command: docker build -t inventory-service .
    run_command: dotnet inventory-service.dll
    http_port: 8080
    instance_count: 1
    instance_size_slug: basic-xxs
    envs:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ConnectionStrings__DefaultConnection
        value: ${db.DATABASE_URL}
databases:
  - name: db
    engine: PG
    version: "16"
    size: db-s-1vcpu-1gb
EOF

# 5. App deployen
doctl apps create --spec .do/app.yaml
```

**Zugriff**: Automatisch generierte URL über DigitalOcean Dashboard

---

## ⚓ Kubernetes Deployment

Für größere Deployments mit hoher Verfügbarkeit und Skalierbarkeit.

### 1. Kubernetes Cluster erstellen

**Option A: Managed Kubernetes (empfohlen)**

```bash
# Google Kubernetes Engine (GKE)
gcloud container clusters create inventory-cluster \
  --zone=europe-west1-b \
  --num-nodes=2 \
  --machine-type=e2-small

# Azure Kubernetes Service (AKS)
az aks create \
  --resource-group inventory-rg \
  --name inventory-cluster \
  --node-count 2 \
  --node-vm-size Standard_B2s

# DigitalOcean Kubernetes
doctl kubernetes cluster create inventory-cluster \
  --region fra1 \
  --node-pool "name=worker-pool;size=s-2vcpu-4gb;count=2"
```

**Option B: Self-Hosted (z.B. mit k3s auf VPS)**

```bash
# Auf Server
curl -sfL https://get.k3s.io | sh -

# Kubeconfig kopieren
sudo cat /etc/rancher/k3s/k3s.yaml
# Fügen Sie diese Konfiguration in ~/.kube/config auf lokalem Rechner ein
```

### 2. Kubernetes Manifests deployen

```bash
# Alle Manifests anwenden
kubectl apply -f k8s/

# Oder einzeln:
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

### 3. Status prüfen

```bash
# Alle Ressourcen anzeigen
kubectl get all -n inventory-service

# Pods überwachen
kubectl get pods -n inventory-service -w

# Logs anzeigen
kubectl logs -n inventory-service deployment/inventory-service -f

# Service-Details
kubectl describe service inventory-service -n inventory-service
```

### 4. Externe IP abrufen

```bash
# LoadBalancer IP
kubectl get service inventory-service -n inventory-service

# Warten bis EXTERNAL-IP verfügbar ist
# Dann zugreifen via: http://EXTERNAL-IP/api/inventory
```

### 5. Ingress Controller installieren (für Domain-Nutzung)

```bash
# NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Cert-Manager für SSL (Let's Encrypt)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.1/cert-manager.yaml

# ClusterIssuer für Let's Encrypt erstellen
cat <<EOF | kubectl apply -f -
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: your-email@example.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

---

## 🌐 Domain & SSL Setup

### 1. Domain-Konfiguration

#### A. Domain kaufen (falls noch keine vorhanden)
- **Namecheap**, **GoDaddy**, **Google Domains**, **Cloudflare**

#### B. DNS-Einträge erstellen

**Für VPS/Server:**
```
A Record:  inventory.example.com  →  SERVER_IP
```

**Für Cloud-Provider:**
```
# Azure
CNAME Record: inventory.example.com → inventory-service-unique.westeurope.azurecontainer.io

# Google Cloud Run
CNAME Record: inventory.example.com → ghs.googlehosted.com
# Dann Domain in Cloud Run Console verifizieren

# DigitalOcean Apps
CNAME Record: inventory.example.com → your-app.ondigitalocean.app
```

### 2. SSL/TLS mit Let's Encrypt (kostenlos)

#### Option A: Nginx Reverse Proxy (auf VPS)

```bash
# 1. Nginx installieren
apt install nginx certbot python3-certbot-nginx -y

# 2. Nginx-Konfiguration
cat > /etc/nginx/sites-available/inventory-service << 'EOF'
server {
    listen 80;
    server_name inventory.example.com;

    location / {
        proxy_pass http://localhost:8081;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
EOF

# 3. Site aktivieren
ln -s /etc/nginx/sites-available/inventory-service /etc/nginx/sites-enabled/
nginx -t
systemctl reload nginx

# 4. SSL-Zertifikat erstellen
certbot --nginx -d inventory.example.com

# 5. Auto-Renewal testen
certbot renew --dry-run
```

#### Option B: Traefik (als Docker Container)

```yaml
# docker-compose.traefik.yml
services:
  traefik:
    image: traefik:v2.10
    container_name: traefik
    command:
      - "--api.insecure=true"
      - "--providers.docker=true"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.letsencrypt.acme.email=your-email@example.com"
      - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
      - "--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web"
    ports:
      - "80:80"
      - "443:443"
      - "8080:8080"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - ./letsencrypt:/letsencrypt
    networks:
      - inventory-network

  inventoryservice:
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.inventory.rule=Host(`inventory.example.com`)"
      - "traefik.http.routers.inventory.entrypoints=websecure"
      - "traefik.http.routers.inventory.tls.certresolver=letsencrypt"
      - "traefik.http.services.inventory.loadbalancer.server.port=8080"
      # HTTP zu HTTPS redirect
      - "traefik.http.routers.inventory-http.rule=Host(`inventory.example.com`)"
      - "traefik.http.routers.inventory-http.entrypoints=web"
      - "traefik.http.routers.inventory-http.middlewares=redirect-to-https"
      - "traefik.http.middlewares.redirect-to-https.redirectscheme.scheme=https"
```

Starten mit:
```bash
docker compose -f docker-compose.production.yml -f docker-compose.traefik.yml up -d
```

---

## 🔐 Sicherheits-Best-Practices

### 1. Sichere Passwörter verwenden

```bash
# Generiere ein sicheres Passwort
openssl rand -base64 32

# Setze es als Umgebungsvariable
export DB_PASSWORD="generated-secure-password"
```

### 2. .env Datei für Secrets

```bash
# .env erstellen
cat > .env << 'EOF'
DB_PASSWORD=your-super-secure-password-here
ASPNETCORE_ENVIRONMENT=Production
EOF

# Permissions setzen
chmod 600 .env

# In .gitignore eintragen
echo ".env" >> .gitignore
```

**docker-compose.production.yml anpassen:**
```yaml
services:
  inventoryservice:
    env_file:
      - .env
```

### 3. Firewall konfigurieren

```bash
# Nur notwendige Ports öffnen
ufw default deny incoming
ufw default allow outgoing
ufw allow 22/tcp    # SSH
ufw allow 80/tcp    # HTTP
ufw allow 443/tcp   # HTTPS
ufw enable
```

### 4. SSH-Sicherheit erhöhen

```bash
# Passwort-Login deaktivieren
echo "PasswordAuthentication no" >> /etc/ssh/sshd_config
systemctl restart sshd

# SSH-Key verwenden (auf lokalem Rechner)
ssh-keygen -t ed25519
ssh-copy-id root@your-server-ip
```

### 5. Regelmäßige Updates

```bash
# Automatische Security-Updates aktivieren
apt install unattended-upgrades -y
dpkg-reconfigure -plow unattended-upgrades
```

### 6. Secrets in Kubernetes

```bash
# Secrets sicher erstellen (nicht in Git!)
kubectl create secret generic inventory-secrets \
  --from-literal=POSTGRES_PASSWORD='your-secure-password' \
  -n inventory-service

# Bestehende Secret-YAML löschen
rm k8s/secrets.yaml
```

### 7. Network Security Groups (Cloud)

- Beschränke Zugriff auf Port 5432 (PostgreSQL) nur auf interne Netzwerke
- Verwende Private Networking für Datenbank-Verbindungen
- Aktiviere DDoS-Protection

---

## 📊 Monitoring & Logs

### 1. Logs auf VPS

```bash
# Docker Logs
docker compose -f docker-compose.production.yml logs -f

# System Logs
journalctl -u inventory-service -f

# Nginx Logs
tail -f /var/log/nginx/access.log
tail -f /var/log/nginx/error.log
```

### 2. Logs in Kubernetes

```bash
# Pod Logs
kubectl logs -n inventory-service deployment/inventory-service -f

# Alle Container in einem Pod
kubectl logs -n inventory-service pod/inventory-service-xxx --all-containers

# Logs von allen Pods
kubectl logs -n inventory-service -l app=inventory-service --all-containers
```

### 3. Einfaches Monitoring Setup

```bash
# Installiere ctop für Container-Monitoring
wget https://github.com/bcicen/ctop/releases/download/v0.7.7/ctop-0.7.7-linux-amd64 -O /usr/local/bin/ctop
chmod +x /usr/local/bin/ctop
ctop
```

### 4. Uptime Monitoring

Kostenlose Services:
- **UptimeRobot** (uptimerobot.com) - 50 Monitore kostenlos
- **Healthchecks.io** - Open Source
- **StatusCake** - Kostenloser Plan verfügbar

```bash
# Beispiel: Health-Check Endpoint
curl https://hc-ping.com/YOUR-UUID  # Healthchecks.io
```

---

## 🚀 Schnellstart-Checkliste

### Für VPS-Deployment (Empfohlen für Anfänger):

- [ ] VPS bei Provider mieten (z.B. Hetzner, DigitalOcean)
- [ ] SSH-Zugang einrichten
- [ ] Docker & Docker Compose installieren
- [ ] Repository klonen
- [ ] Sicheres Passwort generieren und setzen
- [ ] Services mit `docker-compose.production.yml` starten
- [ ] Firewall konfigurieren
- [ ] Domain kaufen und DNS konfigurieren
- [ ] Nginx installieren und konfigurieren
- [ ] SSL-Zertifikat mit Let's Encrypt erstellen
- [ ] Automatischen Start einrichten
- [ ] Monitoring einrichten

### Für Cloud-Deployment:

- [ ] Cloud-Provider auswählen
- [ ] Account erstellen und Zahlungsmethode hinterlegen
- [ ] CLI-Tools installieren
- [ ] Image in Container-Registry pushen
- [ ] Managed Database erstellen
- [ ] Container Service deployen
- [ ] Domain und SSL konfigurieren

---

## 📞 Support & Troubleshooting

### Häufige Probleme

#### 1. "Connection refused" beim Zugriff

**Lösung:**
```bash
# Firewall-Regeln prüfen
ufw status

# Container Status
docker compose ps

# Logs prüfen
docker compose logs
```

#### 2. Datenbank-Verbindung schlägt fehl

**Lösung:**
```bash
# PostgreSQL-Verfügbarkeit prüfen
docker compose exec db-inventory pg_isready -U postgres

# Connection String prüfen
docker compose exec inventoryservice env | grep ConnectionStrings
```

#### 3. SSL-Zertifikat funktioniert nicht

**Lösung:**
```bash
# DNS-Propagation prüfen
nslookup inventory.example.com

# Certbot erneut ausführen
certbot --nginx -d inventory.example.com --force-renewal
```

### Hilfreiche Befehle

```bash
# System-Ressourcen
htop
df -h
free -h

# Netzwerk
netstat -tulpn
ss -tulpn

# Docker-Cleanup
docker system prune -a
```

---

## 📚 Weiterführende Ressourcen

- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Let's Encrypt Guide](https://letsencrypt.org/getting-started/)
- [Nginx Configuration Guide](https://nginx.org/en/docs/)

---

**🎯 Status**: ✅ **Deployment-Ready**  
**📅 Erstellt**: Februar 2026  
**🏷️ Version**: 1.0.0

Viel Erfolg beim Deployment! 🚀
