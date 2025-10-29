# Changelog - Inventory Service

## [1.1.0] - 2025-01-29

### Added
- ? Neues Feld `Name` zum InventoryItem Model hinzugef�gt
- ? Neues Feld `Supplier` zum InventoryItem Model hinzugef�gt
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
- ? Vollst�ndige CRUD-Operationen f�r Lagerartikel
- ? PostgreSQL Integration
- ? Docker & Docker Compose Support
- ? RESTful API mit Swagger
- ? Automatische Datenbankerstellung
