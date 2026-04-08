using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace Myapp.Fournisseurs
{
    public class Fournisseur
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public string Tel { get; set; } = "";
        public string Address { get; set; } = "";
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class FournisseurDTO
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string Tel { get; set; } = "";
        public string Address { get; set; } = "";
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateFournisseurDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Tel { get; set; }
        public string? Address { get; set; }
    }
}
