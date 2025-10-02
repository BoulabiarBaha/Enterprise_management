using MongoDB.Driver;
using Myapp.Models;
using Myapp.Settings;
using Microsoft.Extensions.Options;
using MyApp.Products;
using Myapp.Billings;

namespace Myapp.Transactions
{
    public class TransactionService
    {
        private readonly IMongoCollection<Transaction> _transactions;
        private readonly IMongoCollection<Client> _clients;
        private readonly IMongoCollection<Product> _products;
        private readonly BillingService _billingService; // Ajout du service Billing

        public TransactionService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings, BillingService billingService)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _transactions = database.GetCollection<Transaction>("Transactions");
            _clients = database.GetCollection<Client>("Clients");
            _products = database.GetCollection<Product>("Products");
            _billingService = billingService;
        }

        // Créer une transaction
        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            // Calculer le TotalPrice en fonction des produits vendus
            double totalPrice = 0;
            foreach (var soldProduct in transaction.SoldProducts)
            {
                var product = await _products.Find(p => p.Id == soldProduct.ProductId).FirstOrDefaultAsync();
                if (product == null)
                {
                    throw new Exception($"Product with ID {soldProduct.ProductId} not found.");
                }
                totalPrice += product.UnitPrice * soldProduct.Quantity;
            }
            transaction.TotalPrice = totalPrice;

            // verifier que le client existe
            var client = await _clients.Find(c => c.Id == transaction.ClientId).FirstOrDefaultAsync();
            if (client == null)
            {
                throw new Exception($"Client with ID {transaction.ClientId} not found.");
            }

            // Créer une Facture
            var billing = new Billing
            {
                Reference = $"INV-{DateTime.UtcNow:yyyyMMdd-HHmmss}", //à changer peut etre
                Date = DateTime.UtcNow,
                TotalHT = totalPrice,
                TVA = 0.19,
                TotalTTC = totalPrice + totalPrice * 0.19, //pas optimale
                EnableTax = true
            };

            // Enregistrer la Billing
            var createdBilling = await _billingService.CreateBillingAsyncAutomatic(billing);

            // Ajouter l'ID de la Billing à la Transaction
            transaction.BillingId = createdBilling.Id;

            // Ajouter la transaction à la collection
            await _transactions.InsertOneAsync(transaction);

            // Mettre à jour le client concerné
            client.TransactionIds.Add(transaction.Id); // Ajouter l'ID de la transaction
            client.BillingIds.Add(transaction.BillingId); //Ajouter l'ID de la facture
            client.Value += transaction.TotalPrice; // Mettre à jour la valeur du client
            await _clients.ReplaceOneAsync(c => c.Id == client.Id, client);
            return transaction;
        }

        // Récupérer toutes les transactions
        public async Task<List<Transaction>> GetTransactionsAsync() =>
            await _transactions.Find(_ => true).ToListAsync();

        // Récupérer une transaction par ID
        public async Task<Transaction> GetTransactionAsync(Guid id) =>
            await _transactions.Find(t => t.Id == id).FirstOrDefaultAsync();

        // Modifier une transaction
        public async Task UpdateTransactionAsync(Guid id, Transaction transaction)
        {
            // Get the existing transaction
            var existingTransaction = await _transactions.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTransaction == null)
            {
                throw new Exception("Transaction not found.");
            }
            await _transactions.ReplaceOneAsync(c => c.Id == id, transaction);
        }

        // Supprimer une transaction
        public async Task DeleteTransactionAsync(Guid id) //Supprimer Billing + TransactionIds et BillingIds du clt
        {
            await _transactions.DeleteOneAsync(t => t.Id == id);
        }

        // Map a Transaction to TransactionDTO
        public TransactionDTO MapToTransactionDTO(Transaction transaction)
        {
            var mappedTransactionDTO = new TransactionDTO
            {
                Id = transaction.Id,
                ClientId = transaction.ClientId,
                TotalPrice = transaction.TotalPrice,
                Date = transaction.Date,
                BillingId = transaction.BillingId
            };
            transaction.SoldProducts.ForEach( item => mappedTransactionDTO.SoldProducts.Add(item));
            return mappedTransactionDTO;
        }
        
        // Map a list of Transactions to a list of TransactionsDTOs
        public List<TransactionDTO> MapToListDTOs(List<Transaction> transactions)
        {
            return transactions.Select(transaction => MapToTransactionDTO(transaction)).ToList();
        }
    }
}