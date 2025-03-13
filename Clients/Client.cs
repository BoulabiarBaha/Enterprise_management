using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Myapp.Models
{
    public class Client
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string NumIdentiteFiscal { get; set; }
        public string Tel { get; set; } = "";
        public string Address { get; set; } ="";
        public List<Guid> TransactionIds { get; set; } = new List<Guid>();
        public List<Guid> BillingIds { get; set; } = new List<Guid>();
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ClientDTO
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public required string NumIdentiteFiscal { get; set; }
        public string Tel { get; set; } = "";
        public string Address { get; set; } ="";
        public List<Guid> TransactionIds { get; set; } = new List<Guid>();
        public List<Guid> BillingIds { get; set; } = new List<Guid>();
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class UpdateClientDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Tel { get; set; }
        public string? Address { get; set; }
    }
}