using MongoDB.Driver;
using Myapp.Models;
using Myapp.Settings;
using Microsoft.Extensions.Options;

namespace Myapp.Billings
{
    public class BillingService
    {
        private readonly IMongoCollection<Billing> _billings;

        public BillingService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _billings = database.GetCollection<Billing>("Billings");
        }

        // Réccupéré toutes les factures
        public async Task<List<Billing>> GetClientsAsync() =>
            await _billings.Find(b => true).ToListAsync();

        // Créer une facture
        public async Task<Billing> CreateBillingAsync(Billing billing)
        {
            await _billings.InsertOneAsync(billing);
            return billing;
        }

        // Récupérer une facture par ID
        public async Task<Billing> GetBillingAsync(Guid id) =>
            await _billings.Find(b => b.Id == id).FirstOrDefaultAsync();

        // Supprimer une facture
        public async Task DeleteBillingAsync(Guid id) => 
            await _billings.DeleteOneAsync(b => b.Id == id);
    }
}