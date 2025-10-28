using inventory_service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 🔹 Automatisch Datenbank und Tabellen erstellen/aktualisieren mit Retry-Logik
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryContext>();
    var maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            Console.WriteLine($"Versuch {i + 1}/{maxRetries}: Verbindung zur Datenbank wird hergestellt...");
            
            // Teste die Verbindung
            db.Database.CanConnect();
            
            // Prüfe, ob die InventoryItems-Tabelle existiert
            var tableExists = db.Database.GetService<IRelationalDatabaseCreator>().HasTables();
            
            if (!tableExists)
            {
                Console.WriteLine("Keine Tabellen gefunden, erstelle Datenbank komplett neu...");
                db.Database.EnsureCreated();
                Console.WriteLine("Datenbank und Tabellen erfolgreich erstellt.");
            }
            else
            {
                Console.WriteLine("Datenbank-Tabellen bereits vorhanden.");
                // Versuche Migrationen anzuwenden, falls vorhanden
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("Migrationen erfolgreich angewendet.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Keine Migrationen anzuwenden: {ex.Message}");
                }
            }
            break; // Erfolgreich, verlasse die Schleife
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Verbindungsfehler: {ex.Message}");
            if (i == maxRetries - 1)
            {
                Console.WriteLine("Maximale Anzahl von Verbindungsversuchen erreicht. Anwendung wird beendet.");
                throw;
            }
            Console.WriteLine($"Warte {delay.TotalSeconds} Sekunden vor dem nächsten Versuch...");
            Thread.Sleep(delay);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
