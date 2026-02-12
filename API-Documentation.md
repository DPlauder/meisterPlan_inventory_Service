# 📋 API Dokumentation - Inventory Service

## Übersicht

Die Inventory Service API ermöglicht die vollständige Verwaltung von Lagerartikeln mit CRUD-Operationen (Create, Read, Update, Delete).

**Base URL:** `http://localhost:8081`  
**Content-Type:** `application/json`

---

## 🎯 Endpoints

### 1. 📋 Alle Artikel abrufen

**GET** `/api/inventory`

Gibt eine Liste aller Lagerartikel zurück.

#### Request

```http
GET /api/inventory HTTP/1.1
Host: localhost:8081
Accept: application/json
```

#### Response

**Status:** `200 OK`

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

#### Mögliche Fehler

- `500 Internal Server Error` - Datenbankfehler

---

### 2. 🔍 Einzelnen Artikel abrufen

**GET** `/api/inventory/{artikelnummer}`

Ruft einen spezifischen Artikel anhand seiner Artikelnummer ab.

#### Parameter

| Name            | Typ    | Beschreibung             | Beispiel  |
| --------------- | ------ | ------------------------ | --------- |
| `artikelnummer` | string | Eindeutige Artikelnummer | `ART-001` |

#### Request

```http
GET /api/inventory/ART-001 HTTP/1.1
Host: localhost:8081
Accept: application/json
```

#### Response

**Status:** `200 OK`

```json
{
  "id": 1,
  "articleNumber": "ART-001",
  "quantity": 50,
  "location": "Lager A"
}
```

#### Mögliche Fehler

- `404 Not Found` - Artikel nicht gefunden
- `500 Internal Server Error` - Datenbankfehler

---

### 3. ➕ Neuen Artikel hinzufügen

**POST** `/api/inventory`

Erstellt einen neuen Lagerartikel.

#### Request Body

```json
{
  "articleNumber": "ART-003",
  "quantity": 75,
  "location": "Lager C"
}
```

#### Request

```http
POST /api/inventory HTTP/1.1
Host: localhost:8081
Content-Type: application/json

{
  "articleNumber": "ART-003",
  "quantity": 75,
  "location": "Lager C"
}
```

#### Response

**Status:** `201 Created`

```json
{
  "id": 3,
  "articleNumber": "ART-003",
  "quantity": 75,
  "location": "Lager C"
}
```

**Headers:**

```
Location: /api/inventory/ART-003
```

#### Validierung

- `articleNumber`: Pflichtfeld, max. 50 Zeichen
- `quantity`: Muss >= 0 sein
- `location`: Pflichtfeld, max. 100 Zeichen

#### Mögliche Fehler

- `400 Bad Request` - Ungültige Eingabedaten
- `409 Conflict` - Artikelnummer bereits vorhanden
- `500 Internal Server Error` - Datenbankfehler

---

### 4. ✏️ Artikel-Menge aktualisieren

**PUT** `/api/inventory/{sku}`

Aktualisiert die Menge eines bestehenden Artikels.

#### Parameter

| Name  | Typ    | Beschreibung                                   | Beispiel  |
| ----- | ------ | ---------------------------------------------- | --------- |
| `sku` | string | Artikelnummer des zu aktualisierenden Artikels | `ART-001` |

#### Request Body

```json
150
```

#### Request

```http
PUT /api/inventory/ART-001 HTTP/1.1
Host: localhost:8081
Content-Type: application/json

150
```

#### Response

**Status:** `200 OK`

```json
{
  "id": 1,
  "articleNumber": "ART-001",
  "quantity": 150,
  "location": "Lager A"
}
```

#### Validierung

- `quantity`: Muss >= 0 sein
- `sku`: Muss existieren

#### Mögliche Fehler

- `400 Bad Request` - Ungültige Mengeneingabe
- `404 Not Found` - Artikel nicht gefunden
- `500 Internal Server Error` - Datenbankfehler

---

### 5. 🗑️ Artikel löschen

**DELETE** `/api/inventory/{sku}`

Löscht einen Artikel aus dem Lager.

#### Parameter

| Name  | Typ    | Beschreibung                             | Beispiel  |
| ----- | ------ | ---------------------------------------- | --------- |
| `sku` | string | Artikelnummer des zu löschenden Artikels | `ART-001` |

#### Request

```http
DELETE /api/inventory/ART-001 HTTP/1.1
Host: localhost:8081
```

#### Response

**Status:** `204 No Content`

Keine Response-Body bei erfolgreichem Löschen.

#### Mögliche Fehler

- `404 Not Found` - Artikel nicht gefunden
- `500 Internal Server Error` - Datenbankfehler

---

## 📊 Datenmodell

### InventoryItem Schema

```json
{
  "type": "object",
  "properties": {
    "id": {
      "type": "integer",
      "format": "int32",
      "description": "Eindeutige ID (wird automatisch generiert)",
      "example": 1,
      "readOnly": true
    },
    "articleNumber": {
      "type": "string",
      "maxLength": 50,
      "description": "Eindeutige Artikelnummer",
      "example": "ART-001",
      "required": true
    },
    "quantity": {
      "type": "integer",
      "format": "int32",
      "minimum": 0,
      "description": "Verfügbare Menge im Lager",
      "example": 50,
      "required": true
    },
    "location": {
      "type": "string",
      "maxLength": 100,
      "description": "Lagerort des Artikels",
      "example": "Lager A",
      "required": true
    }
  }
}
```

---

## 🧪 Test-Beispiele

### cURL Kommandos

#### Alle Artikel abrufen

```bash
curl -X GET "http://localhost:8081/api/inventory" \
     -H "Accept: application/json"
```

#### Artikel erstellen

```bash
curl -X POST "http://localhost:8081/api/inventory" \
     -H "Content-Type: application/json" \
     -d '{
       "articleNumber": "TEST-001",
       "quantity": 25,
       "location": "Test-Lager"
     }'
```

#### Artikel aktualisieren

```bash
curl -X PUT "http://localhost:8081/api/inventory/TEST-001" \
     -H "Content-Type: application/json" \
     -d '50'
```

#### Artikel löschen

```bash
curl -X DELETE "http://localhost:8081/api/inventory/TEST-001"
```

### PowerShell Beispiele

#### GET Request

```powershell
$response = Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" -Method GET
$response | Format-Table
```

#### POST Request

```powershell
$body = @{
    ArticleNumber = "PS-001"
    Quantity = 30
    Location = "PowerShell-Lager"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:8081/api/inventory" `
                              -Method POST `
                              -Body $body `
                              -ContentType "application/json"
$response
```

#### PUT Request

```powershell
$newQuantity = 75
Invoke-RestMethod -Uri "http://localhost:8081/api/inventory/PS-001" `
                  -Method PUT `
                  -Body $newQuantity `
                  -ContentType "application/json"
```

---

## 🚦 HTTP Status Codes

| Status Code                 | Bedeutung                | Wann verwendet                  |
| --------------------------- | ------------------------ | ------------------------------- |
| `200 OK`                    | Erfolgreiche Anfrage     | GET, PUT erfolgreich            |
| `201 Created`               | Ressource erstellt       | POST erfolgreich                |
| `204 No Content`            | Erfolgreich, keine Daten | DELETE erfolgreich              |
| `400 Bad Request`           | Ungültige Anfrage        | Validierungsfehler              |
| `404 Not Found`             | Ressource nicht gefunden | Artikel existiert nicht         |
| `409 Conflict`              | Konflikt                 | Artikelnummer bereits vorhanden |
| `500 Internal Server Error` | Serverfehler             | Datenbankprobleme               |

---

## 🔄 Batch-Operationen

Für zukünftige Versionen geplant:

### Mehrere Artikel gleichzeitig hinzufügen

```http
POST /api/inventory/batch
Content-Type: application/json

[
  {
    "articleNumber": "BATCH-001",
    "quantity": 10,
    "location": "Lager A"
  },
  {
    "articleNumber": "BATCH-002",
    "quantity": 20,
    "location": "Lager B"
  }
]
```

### Mehrere Artikel gleichzeitig aktualisieren

```http
PUT /api/inventory/batch
Content-Type: application/json

[
  {
    "articleNumber": "ART-001",
    "quantity": 100
  },
  {
    "articleNumber": "ART-002",
    "quantity": 200
  }
]
```

---

## 🔍 Filterung und Suche

Für zukünftige Versionen geplant:

### Nach Lagerort filtern

```http
GET /api/inventory?location=Lager A
```

### Artikel mit niedrigem Bestand

```http
GET /api/inventory?quantity_lt=10
```

### Textsuche in Artikelnummern

```http
GET /api/inventory?search=ART
```

---

## 📈 Rate Limiting

Derzeit keine Rate Limits implementiert. Für Produktionsumgebungen empfohlen:

- **GET Requests**: 100 pro Minute
- **POST/PUT/DELETE**: 20 pro Minute
- **Burst**: 200 Requests in 5 Sekunden

## 🔐 Authentifizierung

Derzeit keine Authentifizierung implementiert. Für Produktionsumgebungen empfohlen:

- **JWT Bearer Token**
- **API Keys**
- **OAuth 2.0**

Beispiel für zukünftigen Header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📝 Versionierung

**Aktuelle Version**: `v1`

Alle Endpoints sind versioniert über die URL:

- `/api/inventory` (aktuelle Version)
- `/api/v1/inventory` (explizite v1)
- `/api/v2/inventory` (zukünftige v2)

---

**🎯 API Status**: ✅ **Produktionsbereit**  
**📅 Letzte Aktualisierung**: 28. Oktober 2025  
**🏷️ API Version**: 1.0.0
