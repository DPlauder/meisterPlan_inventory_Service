# ?? API Beispiele - Inventory Service (v1.1.0)

## Erweitertes Datenmodell

```json
{
  "id": 1,
  "articleNumber": "ART-001",
  "name": "Laptop Dell XPS 15",
  "quantity": 50,
  "location": "Lager A",
  "supplier": "Dell Technologies"
}
```

## ?? Beispiel-Requests

### 1. Artikel mit vollständigen Informationen erstellen

```bash
curl -X POST "http://localhost:8081/api/inventory" \
     -H "Content-Type: application/json" \
     -d '{
       "articleNumber": "LAP-001",
       "name": "Laptop Dell XPS 15",
       "quantity": 25,
       "location": "Hauptlager",
       "supplier": "Dell Technologies"
     }'
```

**PowerShell:**
```powershell
$body = @{
    ArticleNumber = "LAP-001"
    Name = "Laptop Dell XPS 15"
    Quantity = 25
  Location = "Hauptlager"
    Supplier = "Dell Technologies"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" `
       -Method POST `
       -Body $body `
  -ContentType "application/json"
```

### 2. Mehrere Artikel mit verschiedenen Lieferanten

```powershell
# Artikel 1: Büromaterial
$artikel1 = @{
    ArticleNumber = "PEN-001"
    Name = "Kugelschreiber blau 10er Pack"
    Quantity = 500
    Location = "Kleinteilelager"
    Supplier = "Office Depot"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" `
         -Method POST -Body $artikel1 -ContentType "application/json"

# Artikel 2: Elektronik
$artikel2 = @{
    ArticleNumber = "MON-001"
    Name = "Monitor Dell UltraSharp 27 Zoll"
    Quantity = 30
    Location = "IT-Lager"
    Supplier = "Dell Technologies"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" `
      -Method POST -Body $artikel2 -ContentType "application/json"

# Artikel 3: Möbel
$artikel3 = @{
    ArticleNumber = "DESK-001"
    Name = "Schreibtisch höhenverstellbar 160x80cm"
    Quantity = 15
    Location = "Möbellager"
    Supplier = "IKEA Business"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" `
     -Method POST -Body $artikel3 -ContentType "application/json"
```

### 3. Alle Artikel mit Lieferanten-Info abrufen

```bash
curl -X GET "http://localhost:8081/api/inventory" | jq
```

**Erwartete Response:**
```json
[
  {
    "id": 1,
    "articleNumber": "LAP-001",
 "name": "Laptop Dell XPS 15",
    "quantity": 25,
    "location": "Hauptlager",
    "supplier": "Dell Technologies"
  },
  {
    "id": 2,
    "articleNumber": "PEN-001",
    "name": "Kugelschreiber blau 10er Pack",
    "quantity": 500,
    "location": "Kleinteilelager",
    "supplier": "Office Depot"
  },
  {
    "id": 3,
    "articleNumber": "MON-001",
    "name": "Monitor Dell UltraSharp 27 Zoll",
    "quantity": 30,
    "location": "IT-Lager",
    "supplier": "Dell Technologies"
  }
]
```

### 4. Artikel nach Artikelnummer mit allen Details abrufen

```bash
curl -X GET "http://localhost:8081/api/inventory/LAP-001"
```

**Response:**
```json
{
  "id": 1,
  "articleNumber": "LAP-001",
  "name": "Laptop Dell XPS 15",
  "quantity": 25,
  "location": "Hauptlager",
  "supplier": "Dell Technologies"
}
```

## ?? Testdaten-Script

**PowerShell Script zum Befüllen der Datenbank:**

```powershell
# test-data.ps1
$baseUrl = "http://localhost:8081/api/inventory"

$testArtikel = @(
    @{
  ArticleNumber = "LAP-DELL-001"
   Name = "Dell Latitude 5520 15.6 Zoll"
        Quantity = 12
    Location = "IT-Hauptlager"
        Supplier = "Dell Technologies"
    },
    @{
    ArticleNumber = "LAP-HP-001"
        Name = "HP EliteBook 850 G8"
        Quantity = 8
     Location = "IT-Hauptlager"
        Supplier = "HP Inc."
    },
    @{
        ArticleNumber = "MON-DELL-001"
        Name = "Dell P2422H 24 Zoll Monitor"
        Quantity = 35
        Location = "IT-Hauptlager"
        Supplier = "Dell Technologies"
    },
    @{
        ArticleNumber = "KEY-LOG-001"
        Name = "Logitech MX Keys Tastatur"
        Quantity = 20
        Location = "Zubehörlager"
        Supplier = "Logitech"
 },
    @{
        ArticleNumber = "MOUSE-LOG-001"
        Name = "Logitech MX Master 3 Maus"
    Quantity = 25
        Location = "Zubehörlager"
        Supplier = "Logitech"
    },
 @{
        ArticleNumber = "DESK-IKEA-001"
        Name = "BEKANT Schreibtisch 160x80 weiß"
        Quantity = 10
        Location = "Möbellager A"
        Supplier = "IKEA Business"
    },
    @{
        ArticleNumber = "CHAIR-IKEA-001"
        Name = "MARKUS Bürostuhl schwarz"
        Quantity = 15
    Location = "Möbellager A"
        Supplier = "IKEA Business"
  },
    @{
        ArticleNumber = "PEN-OFFICE-001"
 Name = "Kugelschreiber Schneider blau 50er"
   Quantity = 200
        Location = "Kleinteilelager"
    Supplier = "Office Depot"
 },
    @{
      ArticleNumber = "PAPER-OFFICE-001"
        Name = "Kopierpapier A4 80g 2500 Blatt"
      Quantity = 50
Location = "Papierlager"
        Supplier = "Office Depot"
    },
    @{
      ArticleNumber = "DOCK-DELL-001"
        Name = "Dell WD19 USB-C Docking Station"
Quantity = 18
        Location = "IT-Zubehör"
 Supplier = "Dell Technologies"
    }
)

Write-Host "Erstelle Testdaten für Inventory Service..." -ForegroundColor Cyan

foreach ($artikel in $testArtikel) {
    try {
        $body = $artikel | ConvertTo-Json
        $response = Invoke-RestMethod -Uri $baseUrl -Method POST -Body $body -ContentType "application/json"
  Write-Host "? Erstellt: $($artikel.ArticleNumber) - $($artikel.Name)" -ForegroundColor Green
    }
    catch {
        Write-Host "? Fehler bei $($artikel.ArticleNumber): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`nFertig! Alle Artikel abrufen:" -ForegroundColor Cyan
$allItems = Invoke-RestMethod -Uri $baseUrl -Method GET
$allItems | Format-Table ArticleNumber, Name, Quantity, Location, Supplier -AutoSize
```

**Ausführen:**
```powershell
.\test-data.ps1
```

## ?? Erweiterte Abfragen (geplant für v2.0)

### Nach Lieferant filtern
```bash
GET /api/inventory?supplier=Dell Technologies
```

### Nach Name suchen
```bash
GET /api/inventory?search=Laptop
```

### Kombinierte Filter
```bash
GET /api/inventory?supplier=IKEA&location=Möbellager
```

---

**?? Aktualisiert**: 29. Januar 2025  
**??? Version**: 1.1.0
