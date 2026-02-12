namespace inventory_service.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public string ArticleNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Location { get; set; } = "";
        public string Supplier { get; set; } = "";

    }
}
