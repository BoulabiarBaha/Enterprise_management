using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Myapp.Models;
using Myapp.Settings;
using MyApp.Products;

namespace Myapp.DashboardStats
{
    public class DashboardService
    {
        private readonly IMongoCollection<Product> _productsCollection;
        private readonly IMongoCollection<Client> _clientsCollection;
        private readonly IMongoCollection<Transaction> _transactionsCollection;

        public DashboardService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _productsCollection = database.GetCollection<Product>("Products");
            _clientsCollection = database.GetCollection<Client>("Clients");
            _transactionsCollection = database.GetCollection<Transaction>("Transactions");
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
        {
            // Run all queries in parallel for better performance
            var tasks = new Task[]
            {
            GetTotalProductsAsync(userId),
            GetActiveClientsAsync(userId),
            GetTransactionsStatsAsync(userId),
            GetMonthlyRevenueAsync(userId),
            GetTotalClientsAsync(userId)
            };

            await Task.WhenAll(tasks);

            var totalProductsTask = (Task<int>)tasks[0];
            var activeClientsTask = (Task<int>)tasks[1];
            var transactionStatsTask = (Task<(int count, double revenue, double avg)>)tasks[2];
            var monthlyRevenueTask = (Task<List<MonthlyRevenueDto>>)tasks[3];
            var totalClientsTask = (Task<int>)tasks[4];

            var totalProducts = await totalProductsTask;
            var totalClients = await totalClientsTask;
            var activeClients = await activeClientsTask;
            var (totalTransactions, totalRevenue, avgTransaction) = await transactionStatsTask;
            var monthlyRevenue = await monthlyRevenueTask;

            // Calculate additional metrics
            var conversionRate = totalClients > 0 ?
                (double)activeClients / totalClients * 100 : 0;
             var repurchaseRate = (totalTransactions - activeClients) / totalTransactions * 100;

            return new DashboardStatsDto
            {
                TotalClients = totalClients,
                TotalProducts = totalProducts,
                ActiveClients = activeClients,
                TotalTransactions = totalTransactions,
                TotalRevenue = totalRevenue,
                AverageTransactionValue = avgTransaction,
                ClientConversionRate = conversionRate,
                MonthlyRevenue = monthlyRevenue,
                RepurchaseRate = repurchaseRate,
            };
        }

        private async Task<int> GetTotalProductsAsync(Guid userId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.CreatedBy, userId);
            var count = await _productsCollection.CountDocumentsAsync(filter);
            return (int)count;
        }

        private async Task<int> GetActiveClientsAsync(Guid userId)
        {
            // Active clients = value > 0
            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Eq(c => c.CreatedBy, userId),
                Builders<Client>.Filter.Gt(c => c.Value, 0)
            );

            var count = await _clientsCollection.CountDocumentsAsync(filter);
            return (int)count;
        }

        private async Task<(int count, double revenue, double average)> GetTransactionsStatsAsync(Guid userId)
        {
            var filter = Builders<Transaction>.Filter.Eq(t => t.CreatedBy, userId);

            var transactions = await _transactionsCollection
                .Find(filter)
                .ToListAsync();

            var count = transactions.Count;
            var revenue = transactions.Sum(t => t.TotalPrice);
            var average = count > 0 ? revenue / count : 0;

            return (count, revenue, average);
        }

        private async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(Guid userId)
        {
            // Get revenue grouped by month for the last 3 months
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);

            var filter = Builders<Transaction>.Filter.And(
                Builders<Transaction>.Filter.Eq(t => t.CreatedBy, userId),
                Builders<Transaction>.Filter.Gte(t => t.Date, threeMonthsAgo)
            );

            var transactions = await _transactionsCollection
                .Find(filter)
                .Project(t => new { t.Date, t.TotalPrice })
                .ToListAsync();

            var monthlyRevenue = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:00}",
                    Revenue = g.Sum(t => t.TotalPrice)
                })
                .OrderByDescending(m => m.Month)
                .Take(3)
                .ToList();

            return monthlyRevenue;
        }

        // Helper method to get total clients for conversion rate
        private async Task<int> GetTotalClientsAsync(Guid userId)
        {
            var filter = Builders<Client>.Filter.Eq(c => c.CreatedBy, userId);
            var count = await _clientsCollection.CountDocumentsAsync(filter);
            return (int)count;
        }

    }

    
}
