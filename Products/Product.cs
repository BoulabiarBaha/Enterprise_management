namespace MyApp.Products
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required double UnitPrice { get; set; }
        public List<PriceChange> PriceHistory { get; set; } = new List<PriceChange>();
        public string Description { get; set; } = "";
        public required string Supplier { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class PriceChange
    {
        public double Price { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

        public class ProductDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required double UnitPrice { get; set; }
        public List<PriceChangeDTO> PriceHistory { get; set; } = new List<PriceChangeDTO>();
        public required string Description { get; set; }
        public required string Supplier { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class PriceChangeDTO
    {
        public double Price { get; set; }
        public DateTime Date { get; set; }
    }
}