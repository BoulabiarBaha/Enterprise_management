using MongoDB.Bson.Serialization.Attributes;

namespace Myapp.Models
{
    public class Billing
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Reference { get; set; } // Référence de la facture
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public double TotalHT { get; set; } // Total hors taxes
        public double TVA { get; set; } // Taux de TVA
        public double TotalTTC { get; set; } // Total toutes taxes comprises
        public bool EnableTax { get; set; } // Indique si la TVA est appliquée
    }
    
    public class BillingDTO
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Reference { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public double TotalHT { get; set; } 
        public double TVA { get; set; } 
        public double TotalTTC { get; set; } 
        public bool EnableTax { get; set; } 
    }
}