using Microsoft.EntityFrameworkCore;
using inventory_service.Models;

namespace inventory_service.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options)
            : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }
    }
}
