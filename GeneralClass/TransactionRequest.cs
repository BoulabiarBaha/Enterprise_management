using Myapp.Models;

namespace MyApp.GeneralClass{
    public class TransactionRequest
    {
        public Guid ClientId { get; set; } // Référence au client
        public List<SoldProduct> SoldProducts { get; set; } = new List<SoldProduct>();
        public bool EnableTax { get; set; } = true; // Activer/désactiver la TVA
        public double TVA { get; set; } = 0.19; // Taux de TVA (par défaut 19%)
    }
}