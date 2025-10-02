using MongoDB.Driver;
using Myapp.Models;
using Myapp.Settings;
using Microsoft.Extensions.Options;
using Myapp.Transactions;

namespace Myapp.Billings
{
    public class BillingService
    {
        private readonly IMongoCollection<Billing> _billings;
        private readonly IMongoCollection<Transaction> _transactions;

        public BillingService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _billings = database.GetCollection<Billing>("Billings");
            _transactions = database.GetCollection<Transaction>("Transactions");
        }

        // Réccupéré toutes les factures
        public async Task<List<Billing>> GetClientsAsync() =>
            await _billings.Find(b => true).ToListAsync();

        // Créer une facture automatiquement lors de création d'une transaction
        public async Task<Billing> CreateBillingAsyncAutomatic(Billing billing)
        {
            await _billings.InsertOneAsync(billing);
            return billing;
        }

        // Créer une facture manuellement (ajout de l'id de la facture dans la transaction déja crée)
        public async Task<Billing> CreateBillingAsyncManually(Billing billing, Guid transactionId)
        {
            // Vérifier que la Transaction existe
            var transaction = await _transactions.Find(t => t.Id == transactionId).FirstOrDefaultAsync();
            if (transaction == null)
            {
                throw new Exception($"Transaction with ID {transactionId} not found.");
            }

            // Enregistrer la facture
            await _billings.InsertOneAsync(billing);

            // Mettre à jour la Transaction avec l'ID de la facture
            transaction.BillingId = billing.Id;
            await _transactions.ReplaceOneAsync(t => t.Id == transactionId, transaction);

            return billing;
        }

        // Récupérer une facture par ID
        public async Task<Billing> GetBillingAsync(Guid id) =>
            await _billings.Find(b => b.Id == id).FirstOrDefaultAsync();

        // Modifier une facture
        public async Task UpdateBillingAsync(Guid id, Billing billing)
        {
            // Vérifier que la facture existe
            var existingBilling = await _billings.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (existingBilling == null)
            {
                throw new Exception($"Billing with ID {id} not found.");
            }

            await _billings.ReplaceOneAsync(b => b.Id == id, billing);
        }

        // Supprimer une facture
        public async Task DeleteBillingAsync(Guid id)
        {
            // Vérifier que la facture existe
            var billing = await _billings.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (billing == null)
            {
                throw new Exception($"Billing with ID {id} not found.");
            }

            // Supprimer la référence dans la Transaction associée
            var transaction = await _transactions.Find(t => t.BillingId == id).FirstOrDefaultAsync();
            if (transaction != null)
            {
                transaction.BillingId = Guid.Empty; // Ou null si BillingId est nullable
                await _transactions.ReplaceOneAsync(t => t.Id == transaction.Id, transaction);
            }

            await _billings.DeleteOneAsync(b => b.Id == id);
        }

        // Récupérer une facture par TransactionId
        public async Task<Billing> GetBillingByTransactionIdAsync(Guid transactionId)
        {
            // Trouver la Transaction
            var transaction = await _transactions.Find(t => t.Id == transactionId).FirstOrDefaultAsync();
            if (transaction == null)
            {
                throw new Exception($"Transaction with ID {transactionId} not found.");
            }

            // Récupérer la Billing associée
            var billing = await _billings.Find(b => b.Id == transaction.BillingId).FirstOrDefaultAsync();
            if (billing == null)
            {
                throw new Exception($"Billing not found for Transaction ID {transactionId}.");
            }

            return billing;
        }

        // Map a billing to a billingDTO
        public BillingDTO MapToBillingDTO(Billing billing)
        {
            var mappedBillingDTO = new BillingDTO
            {
                Id = billing.Id,
                Reference = billing.Reference,
                Date = billing.Date,
                TotalHT = billing.TotalHT,
                TVA = billing.TVA,
                TotalTTC = billing.TotalTTC,
                EnableTax = billing.EnableTax
            };
            return mappedBillingDTO;
        }
        
        // Map a list of Billings to a list of BillingDTOs
        public List<BillingDTO> MapToListBillingDTOs(List<Billing> billings)
        {
            return billings.Select(billling => MapToBillingDTO(billling)).ToList();
        }
    }
}