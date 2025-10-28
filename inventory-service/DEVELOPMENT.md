# 👨‍💻 Development Guide - Inventory Service

## Übersicht

Entwicklungsleitfaden für den Inventory Service Mikroservice mit .NET 8.0, Entity Framework Core und PostgreSQL.

---

## 🏗️ Projektstruktur

```
inventory-service/
├── Controllers/           # API Controller
│   └── InventoryController.cs
├── Data/                 # Datenbank-Context
│   └── InventoryContext.cs
├── Models/               # Datenmodelle
│   └── InventoryItem.cs
├── Properties/           # Launch-Konfiguration
│   └── launchSettings.json
├── bin/                 # Kompilierte Binärdateien
├── obj/                 # Build-Artefakte
├── Migrations/          # EF Core Migrationen (leer)
├── Program.cs           # Application Entry Point
├── appsettings.json     # App-Konfiguration
├── appsettings.Development.json
├── inventory-service.csproj
├── Dockerfile           # Container-Definition
├── docker-compose.yml   # Multi-Container Setup
└── README.md           # Projektdokumentation
```

---

## 🛠️ Entwicklungsumgebung Setup

### Voraussetzungen

#### Software

- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** oder **VS Code**
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** für Versionskontrolle

#### VS Code Extensions (empfohlen)

- **C# Dev Kit** - Offizielle Microsoft Extension
- **Docker** - Container-Management
- **REST Client** - API-Tests
- **GitLens** - Enhanced Git Integration

### Lokale Entwicklung

#### 1. Repository Setup

```bash
# Repository klonen
git clone <repository-url>
cd inventory-service

# Dependencies wiederherstellen
dotnet restore

# Projekt kompilieren
dotnet build
```

#### 2. Datenbank Setup (lokal)

```bash
# PostgreSQL mit Docker starten
docker run --name postgres-dev \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=inventorydb \
  -p 5432:5432 \
  -d postgres:16

# Connection String in appsettings.Development.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=inventorydb;Username=postgres;Password=postgres"
  }
}
```

#### 3. Anwendung starten

```bash
# Development Server starten
dotnet run

# Mit spezifischem Profil
dotnet run --launch-profile "inventory-service"

# Watch Mode (Auto-Reload)
dotnet watch run
```

---

## 🧩 Code-Architektur

### Dependency Injection Container

**Program.cs - Service Registration:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Datenbank-Context registrieren
builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

### Entity Framework Core

#### DbContext Konfiguration

```csharp
public class InventoryContext : DbContext
{
    public InventoryContext(DbContextOptions<InventoryContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API Konfiguration
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ArticleNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.ArticleNumber).IsUnique();
        });
    }
}
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryContext _context;

    public InventoryController(InventoryContext context)
    {
        _context = context;
    }

    // Async/Await Pattern für bessere Performance
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll()
    {
        return await _context.InventoryItems.ToListAsync();
    }
}
```

---

## 🗄️ Datenbank-Management

### Entity Framework Migrationen

```bash
# Migration erstellen
dotnet ef migrations add InitialCreate

# Datenbank aktualisieren
dotnet ef database update

# Migration rückgängig machen
dotnet ef database update PreviousMigration

# Datenbank löschen
dotnet ef database drop
```

### Seed Data (Optional)

**InventoryContext.cs erweitern:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Seed Data
    modelBuilder.Entity<InventoryItem>().HasData(
        new InventoryItem
        {
            Id = 1,
            ArticleNumber = "SEED-001",
            Quantity = 100,
            Location = "Hauptlager"
        }
    );
}
```

### Datenbank-Initialisierung

**Program.cs - Automatische Schema-Erstellung:**

```csharp
// Automatisch Datenbank und Tabellen erstellen/aktualisieren
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            Console.WriteLine($"Versuch {i + 1}/{maxRetries}: Verbindung zur Datenbank...");

            if (db.Database.CanConnect())
            {
                var databaseCreator = db.GetService<IRelationalDatabaseCreator>();
                if (!databaseCreator.HasTables())
                {
                    Console.WriteLine("Erstelle Datenbank-Schema...");
                    db.Database.EnsureCreated();
                }
                break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Verbindungsfehler: {ex.Message}");
            if (i == maxRetries - 1) throw;
            Thread.Sleep(delay);
        }
    }
}
```

---

## 🧪 Testing

### Unit Tests Setup

#### Test-Projekt erstellen

```bash
# Test-Projekt hinzufügen
dotnet new xunit -n inventory-service.Tests

# NuGet-Pakete installieren
cd inventory-service.Tests
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add reference ../inventory-service/inventory-service.csproj
```

#### Controller Unit Test Beispiel

```csharp
public class InventoryControllerTests
{
    private InventoryContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<InventoryContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new InventoryContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsAllItems()
    {
        // Arrange
        using var context = GetInMemoryContext();
        context.InventoryItems.AddRange(
            new InventoryItem { ArticleNumber = "TEST-1", Quantity = 10, Location = "A" },
            new InventoryItem { ArticleNumber = "TEST-2", Quantity = 20, Location = "B" }
        );
        await context.SaveChangesAsync();

        var controller = new InventoryController(context);

        // Act
        var result = await controller.GetAll();

        // Assert
        result.Value.Should().HaveCount(2);
    }
}
```

### Integration Tests

```csharp
public class InventoryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InventoryIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetInventory_ReturnsOkResponse()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/inventory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### Tests ausführen

```bash
# Alle Tests
dotnet test

# Mit Code Coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific Test
dotnet test --filter "GetAll_ReturnsAllItems"
```

---

## 🔧 Konfiguration

### appsettings.json Struktur

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=inventorydb;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "AllowedHosts": "*",
  "InventoryService": {
    "MaxItemsPerRequest": 1000,
    "DefaultLocation": "Hauptlager",
    "EnableAuditLog": true
  }
}
```

### Environment-spezifische Konfiguration

**appsettings.Development.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=inventorydb_dev;Username=postgres;Password=postgres"
  }
}
```

**appsettings.Production.json:**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
  }
}
```

### Konfiguration in Code verwenden

```csharp
public class InventoryService
{
    private readonly IConfiguration _configuration;

    public InventoryService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int GetMaxItemsPerRequest()
    {
        return _configuration.GetValue<int>("InventoryService:MaxItemsPerRequest", 100);
    }
}
```

---

## 📊 Logging & Monitoring

### Structured Logging

```csharp
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(InventoryContext context, ILogger<InventoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Add(InventoryItem item)
    {
        _logger.LogInformation("Adding new item {ArticleNumber} with quantity {Quantity}",
            item.ArticleNumber, item.Quantity);

        try
        {
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully added item {ArticleNumber}", item.ArticleNumber);
            return CreatedAtAction(nameof(GetBySku), new { artikleNum = item.ArticleNumber }, item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add item {ArticleNumber}", item.ArticleNumber);
            throw;
        }
    }
}
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

// Health Check Endpoint
app.MapHealthChecks("/health");
```

---

## 🔄 Development Workflow

### Git Workflow

```bash
# Feature Branch erstellen
git checkout -b feature/new-endpoint

# Änderungen committen
git add .
git commit -m "Add new endpoint for bulk operations"

# Branch pushen
git push origin feature/new-endpoint

# Pull Request erstellen (GitHub/GitLab)
```

### Code Formatierung

**.editorconfig:**

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{json,yml,yaml}]
indent_size = 2
```

### Pre-commit Hooks

```bash
# dotnet-format installieren
dotnet tool install -g dotnet-format

# Code formatieren
dotnet format

# Tests vor Commit ausführen
dotnet test --no-build
```

---

## 🚀 Deployment

### Build für Production

```bash
# Release Build
dotnet publish -c Release -o ./publish

# Mit Runtime-spezifisch
dotnet publish -c Release -r linux-x64 --self-contained false
```

### Docker Build (mehrstufig)

```dockerfile
# Development Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build
ENTRYPOINT ["dotnet", "watch", "run"]

# Production Stage (Multi-stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["inventory-service.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "inventory-service.dll"]
```

---

## 🐛 Debugging

### Visual Studio Code

**.vscode/launch.json:**

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/inventory-service.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

**.vscode/tasks.json:**

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "${workspaceFolder}/inventory-service.csproj"],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

### Container Debugging

```bash
# Container mit Debug-Modus starten
docker run -it --rm \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  inventory-service:latest

# In laufenden Container einsteigen
docker exec -it inventoryservice /bin/bash

# Logs in Echtzeit
docker logs -f inventoryservice
```

---

## 📚 Best Practices

### Code Qualität

1. **Async/Await überall verwenden**
2. **Dependency Injection nutzen**
3. **Exception Handling implementieren**
4. **Input Validation hinzufügen**
5. **Logging strukturiert einsetzen**

### Performance

1. **Database Queries optimieren**
2. **Connection Pooling nutzen**
3. **Caching implementieren (Redis)**
4. **Pagination für große Datasets**

### Sicherheit

1. **Input Sanitization**
2. **SQL Injection Prevention** (EF Core hilft)
3. **Authentication/Authorization**
4. **HTTPS in Production**
5. **Secrets Management**

---

## 🔧 Erweiterte Features

### Custom Middleware

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request: {Method} {Path}",
            context.Request.Method, context.Request.Path);

        await _next(context);

        _logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
    }
}

// In Program.cs registrieren
app.UseMiddleware<RequestLoggingMiddleware>();
```

### Custom Services

```csharp
public interface IInventoryService
{
    Task<InventoryItem> GetByArticleNumberAsync(string articleNumber);
    Task<bool> IsLowStockAsync(string articleNumber, int threshold = 10);
}

public class InventoryService : IInventoryService
{
    private readonly InventoryContext _context;

    public InventoryService(InventoryContext context)
    {
        _context = context;
    }

    public async Task<InventoryItem> GetByArticleNumberAsync(string articleNumber)
    {
        return await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ArticleNumber == articleNumber);
    }

    public async Task<bool> IsLowStockAsync(string articleNumber, int threshold = 10)
    {
        var item = await GetByArticleNumberAsync(articleNumber);
        return item?.Quantity <= threshold;
    }
}

// In Program.cs registrieren
builder.Services.AddScoped<IInventoryService, InventoryService>();
```

---

**🎯 Status**: ✅ **Aktiv entwickelt**  
**📅 Letzte Aktualisierung**: 28. Oktober 2025  
**🏷️ Development Version**: 1.0.0
