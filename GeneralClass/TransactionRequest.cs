using Myapp.Models;

namespace MyApp.GeneralClass{
    public class TransactionRequest
    {
        public Guid ClientId { get; set; } // Référence au client
        public List<SoldProduct> SoldProducts { get; set; } = new List<SoldProduct>();
        public Guid BillingId { get; set; } // Gardé vide pour le moment
    }
}