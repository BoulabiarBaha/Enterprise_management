using MongoDB.Bson.Serialization.Attributes;

namespace Myapp.Models
{
    public class Transaction
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; } // Référence au client
        public List<SoldProduct> SoldProducts { get; set; } = new List<SoldProduct>();
        public double TotalPrice { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid BillingId { get; set; } // Gardé vide pour le moment
        public Guid CreatedBy { get; set; }
    }

    public class SoldProduct
    {
        public Guid ProductId { get; set; } // Référence au produit
        public int Quantity { get; set; }
        public string Note { get; set; } = "";
    }

    public class TransactionDTO
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; } // Référence au client
        public List<SoldProduct> SoldProducts { get; set; } = new List<SoldProduct>();
        public double TotalPrice { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Guid BillingId { get; set; } // Gardé vide pour le moment
        public Guid CreatedBy { get; set; }
    }
}