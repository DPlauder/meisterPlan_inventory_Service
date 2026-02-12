# Changelog - Inventory Service

## [1.1.0] - 2025-01-29

### Added
- ? Neues Feld `Name` zum InventoryItem Model hinzugefügt
- ? Neues Feld `Supplier` zum InventoryItem Model hinzugefügt
- ??? Migration `AddNameAndSupplierToInventoryItem` erstellt

### Changed
- ?? API-Dokumentation aktualisiert mit neuen Feldern
- ?? README mit erweiterten Beispielen aktualisiert

### Migration
```bash
dotnet ef migrations add AddNameAndSupplierToInventoryItem
dotnet ef database update
```

## [1.0.0] - 2025-01-28

### Initial Release
- ? Vollständige CRUD-Operationen für Lagerartikel
- ? PostgreSQL Integration
- ? Docker & Docker Compose Support
- ? RESTful API mit Swagger
- ? Automatische Datenbankerstellung
