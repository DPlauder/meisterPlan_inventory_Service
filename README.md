# 📦 Inventory Service - Mikroservice

Ein **ASP.NET Core 8.0** Mikroservice zur Verwaltung von Lagerbeständen mit **PostgreSQL** Datenbankanbindung und **Docker**-Containerisierung.

## 🚀 Features

- ✅ **Vollständige CRUD-Operationen** für Lagerartikel
- ✅ **Automatische Datenbankerstellung** beim ersten Start
- ✅ **Retry-Logik** für robuste Datenbankverbindungen
- ✅ **PostgreSQL 16** Integration
- ✅ **Docker & Docker Compose** Support
- ✅ **RESTful API** mit Swagger-Dokumentation
- ✅ **Health Checks** für Container-Orchestrierung
- ✅ **Entity Framework Core** mit Code-First Ansatz

## 🏗️ Architektur

```
inventory-service/
├── Controllers/
│   └── InventoryController.cs    # REST API Endpoints
├── Data/
│   └── InventoryContext.cs       # EF Core DbContext
├── Models/
│   └── InventoryItem.cs          # Datenmodell
├── Properties/
│   └── launchSettings.json       # Launch-Konfiguration
├── Program.cs                    # Application Entry Point
├── inventory-service.csproj      # Projekt-Konfiguration
├── Dockerfile                    # Container-Definition
├── docker-compose.yml            # Multi-Container Setup
└── README.md                     # Diese Dokumentation
```

## 📋 Datenmodell

### InventoryItem

```csharp
public class InventoryItem
{
    public int Id { get; set; }              // Primärschlüssel
    public string ArticleNumber { get; set; } // Artikelnummer (eindeutig)
    public int Quantity { get; set; }         // Verfügbare Menge
    public string Location { get; set; }      // Lagerort
}
```

## 🔌 API Endpoints

### Base URL: `http://localhost:8081/api/inventory`

| **HTTP Method** | **Endpoint**       | **Beschreibung**            | **Request Body** | **Response**           |
| --------------- | ------------------ | --------------------------- | ---------------- | ---------------------- |
| `GET`           | `/`                | Alle Artikel abrufen        | -                | `Array<InventoryItem>` |
| `GET`           | `/{artikelnummer}` | Artikel nach Nummer abrufen | -                | `InventoryItem`        |
| `POST`          | `/`                | Neuen Artikel hinzufügen    | `InventoryItem`  | `InventoryItem`        |
| `PUT`           | `/{sku}`           | Artikel-Menge aktualisieren | `int (quantity)` | `InventoryItem`        |
| `DELETE`        | `/{sku}`           | Artikel löschen             | -                | `204 No Content`       |

### 📝 API Beispiele

#### 1. Alle Artikel abrufen

```bash
curl -X GET http://localhost:8081/api/inventory
```

**Response:**

```json
[
  {
    "id": 1,
    "articleNumber": "ART-001",
    "quantity": 50,
    "location": "Lager A"
  },
  {
    "id": 2,
    "articleNumber": "ART-002",
    "quantity": 100,
    "location": "Lager B"
  }
]
```

#### 2. Einzelnen Artikel abrufen

```bash
curl -X GET http://localhost:8081/api/inventory/ART-001
```

#### 3. Neuen Artikel hinzufügen

```bash
curl -X POST http://localhost:8081/api/inventory \
  -H "Content-Type: application/json" \
  -d '{
    "articleNumber": "ART-003",
    "quantity": 75,
    "location": "Lager C"
  }'
```

#### 4. Artikel-Menge aktualisieren

```bash
curl -X PUT http://localhost:8081/api/inventory/ART-001 \
  -H "Content-Type: application/json" \
  -d '150'
```

#### 5. Artikel löschen

```bash
curl -X DELETE http://localhost:8081/api/inventory/ART-001
```

## 🐳 Docker Setup

### Voraussetzungen

- **Docker** >= 20.10
- **Docker Compose** >= 2.0

### Services

- **inventory-service**: ASP.NET Core 8.0 API (Port: 8081)
- **db-inventory**: PostgreSQL 16 Datenbank (Port: 5434)

### Schnellstart

1. **Repository klonen und in Verzeichnis wechseln:**

   ```bash
   cd inventory-service
   ```

2. **Services starten:**

   ```bash
   docker compose up --build -d
   ```

3. **Status überprüfen:**

   ```bash
   docker compose ps
   ```

4. **API testen:**

   ```bash
   curl http://localhost:8081/api/inventory
   ```

5. **Services stoppen:**

   ```bash
   docker compose down
   ```

6. **Services stoppen und Daten löschen:**
   ```bash
   docker compose down -v
   ```

## ⚙️ Konfiguration

### Umgebungsvariablen (docker-compose.yml)

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development
  - ConnectionStrings__DefaultConnection=Host=db-inventory;Database=inventorydb;Username=postgres;Password=postgres
```

### Datenbank-Konfiguration

```yaml
db-inventory:
  image: postgres:16
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
    POSTGRES_DB: inventorydb
  ports:
    - "5434:5432"
```

### Health Check

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres -d inventorydb"]
  interval: 5s
  timeout: 5s
  retries: 5
```

## 🧪 Testing

### Manuelle API-Tests (PowerShell)

#### Alle Artikel abrufen:

```powershell
Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" -Method GET
```

#### Neuen Artikel hinzufügen:

```powershell
$body = @{
    ArticleNumber = "TEST-001"
    Quantity = 25
    Location = "Test-Lager"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" -Method POST -Body $body -ContentType "application/json"
```

#### Artikel-Menge aktualisieren:

```powershell
Invoke-RestMethod -Uri "http://localhost:8081/api/inventory/TEST-001" -Method PUT -Body "50" -ContentType "application/json"
```

### Swagger UI

Öffne im Browser: http://localhost:8081/swagger

## 🔧 Entwicklung

### Lokale Entwicklung (ohne Docker)

1. **PostgreSQL Datenbank starten:**

   ```bash
   docker run --name postgres-dev -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=inventorydb -p 5432:5432 -d postgres:16
   ```

2. **Connection String in appsettings.json anpassen:**

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=inventorydb;Username=postgres;Password=postgres"
     }
   }
   ```

3. **Anwendung starten:**
   ```bash
   dotnet run
   ```

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run project
dotnet run

# Publish release
dotnet publish -c Release
```

## 📊 Monitoring & Logs

### Container Logs anzeigen:

```bash
# Alle Services
docker compose logs

# Nur Inventory Service
docker compose logs inventoryservice

# Live Logs verfolgen
docker compose logs -f inventoryservice
```

### Container Status:

```bash
docker compose ps
```

### Ressourcenverbrauch:

```bash
docker stats
```

## 🚨 Troubleshooting

### Häufige Probleme und Lösungen

#### 1. Port bereits belegt (8081)

**Symptom:** `Bind for 0.0.0.0:8081 failed: port is already allocated`

**Lösung:** Port in docker-compose.yml ändern:

```yaml
ports:
  - "8082:8080" # Externen Port auf 8082 ändern
```

#### 2. Datenbankverbindung fehlgeschlagen

**Symptom:** `Failed to connect to database`

**Lösung:**

```bash
# Container neustarten
docker compose restart inventoryservice

# Oder kompletter Neustart
docker compose down && docker compose up -d
```

#### 3. Datenbank-Schema Probleme

**Symptom:** `relation "InventoryItems" does not exist`

**Lösung:**

```bash
# Datenbank-Volume löschen und neu erstellen
docker compose down -v
docker compose up --build -d
```

#### 4. Service startet nicht

**Symptom:** Container exits immediately

**Lösung:**

```bash
# Detaillierte Logs anzeigen
docker compose logs inventoryservice

# Image neu bauen
docker compose build --no-cache inventoryservice
```

## 🔐 Sicherheit

### Produktions-Empfehlungen

1. **Sichere Passwörter verwenden:**

   ```yaml
   environment:
     POSTGRES_PASSWORD: ${DB_PASSWORD} # Aus Umgebungsvariable
   ```

2. **HTTPS aktivieren:**

   ```yaml
   environment:
     ASPNETCORE_URLS: "https://+:443;http://+:80"
     ASPNETCORE_Kestrel__Certificates__Default__Path: "/certs/cert.pfx"
     ASPNETCORE_Kestrel__Certificates__Default__Password: "${CERT_PASSWORD}"
   ```

3. **Database-Verbindung verschlüsseln:**
   ```
   Host=db-inventory;Database=inventorydb;Username=postgres;Password=postgres;SslMode=Require
   ```

## 📈 Performance

### Optimierungen

1. **Connection Pooling** (bereits aktiviert)
2. **Async/Await** Pattern (implementiert)
3. **Entity Framework Query Optimierung**
4. **Container Resource Limits:**
   ```yaml
   deploy:
     resources:
       limits:
         memory: 512M
         cpus: "0.5"
   ```

## 🔄 CI/CD Integration

### GitHub Actions Beispiel (.github/workflows/docker.yml):

```yaml
name: Docker Build and Deploy
on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build Docker image
        run: docker build -t inventory-service:latest .
      - name: Run tests
        run: docker compose -f docker-compose.test.yml up --abort-on-container-exit
```

## 📚 Zusätzliche Ressourcen

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Compose Reference](https://docs.docker.com/compose/)

## 🤝 Contributing

1. Fork das Repository
2. Erstelle einen Feature Branch (`git checkout -b feature/amazing-feature`)
3. Commit deine Änderungen (`git commit -m 'Add amazing feature'`)
4. Push zum Branch (`git push origin feature/amazing-feature`)
5. Öffne einen Pull Request

## 📄 License

Dieses Projekt steht unter der MIT License. Siehe `LICENSE` Datei für Details.

## 👥 Autoren

- **Entwickler**: GitHub Copilot Assistant
- **Projekt**: MeisterPlan Mikroservices

---

## 🌐 Deployment ins Internet

Um diesen Mikroservice über das Internet erreichbar zu machen, siehe **[DEPLOYMENT.md](DEPLOYMENT.md)**.

### Schnellstart-Optionen:

1. **VPS/Server (Empfohlen für Einsteiger)**
   - Einfachste Methode
   - Ab €5/Monat (Hetzner, DigitalOcean)
   - Docker Compose + Nginx + Let's Encrypt
   - Siehe: [VPS Deployment Guide](DEPLOYMENT.md#vpsserver-deployment-docker-compose)

2. **Cloud-Plattformen**
   - **Google Cloud Run**: Serverless, pay-per-use
   - **Azure Container Instances**: Schnell und einfach
   - **AWS ECS**: Enterprise-ready
   - Siehe: [Cloud Deployment Guide](DEPLOYMENT.md#cloud-plattform-deployment)

3. **Kubernetes**
   - Für hohe Verfügbarkeit und Skalierbarkeit
   - Manifests verfügbar im `k8s/` Verzeichnis
   - Siehe: [Kubernetes Guide](DEPLOYMENT.md#kubernetes-deployment)

📖 **Vollständige Anleitung**: [DEPLOYMENT.md](DEPLOYMENT.md)

---

## 📞 Support

Bei Fragen oder Problemen:

1. Prüfe die Troubleshooting-Sektion
2. Schaue in die Container-Logs
3. Öffne ein GitHub Issue
4. Kontaktiere das Entwicklungsteam

---

**🎯 Status**: ✅ **Produktionsbereit**  
**📅 Letzte Aktualisierung**: Februar 2026  
**🏷️ Version**: 1.0.0
