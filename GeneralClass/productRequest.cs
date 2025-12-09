namespace MyApp.GeneralClass
{
    public class ProductRequest
    {
        public required string Name { get; set; }
        public required double UnitPrice { get; set; }
        public string Description { get; set; } = "";
        public required string Supplier { get; set; }
        public Guid CreatedBy { get; set; }
    }
}