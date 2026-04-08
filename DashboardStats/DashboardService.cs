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
            GetTotalClientsAsync(userId),
            GetProductCoverageRateAsync(userId),
            GetMonthlyClientsAsync(userId)
            };

            await Task.WhenAll(tasks);

            var totalProductsTask = (Task<int>)tasks[0];
            var activeClientsTask = (Task<int>)tasks[1];
            var transactionStatsTask = (Task<(int count, double revenue, double avg)>)tasks[2];
            var monthlyRevenueTask = (Task<List<MonthlyRevenueDto>>)tasks[3];
            var totalClientsTask = (Task<int>)tasks[4];
            var productCoverageTask = (Task<double>)tasks[5];
            var monthlyClientsTask = (Task<List<MonthlyClientsDto>>)tasks[6];

            var totalProducts = await totalProductsTask;
            var totalClients = await totalClientsTask;
            var activeClients = await activeClientsTask;
            var (totalTransactions, totalRevenue, avgTransaction) = await transactionStatsTask;
            var monthlyRevenue = await monthlyRevenueTask;
            var productCoverageRate = await productCoverageTask;
            var monthlyClients = await monthlyClientsTask;

            // Calculate additional metrics
            var conversionRate = totalClients > 0 ?
                (double)activeClients / totalClients * 100 : 0;

            var repurchaseClients = await GetRepurchaseClientsAsync(userId);
            var repurchaseRate = (double)activeClients > 0 ? repurchaseClients / activeClients * 100 : 0;

            // Calculate revenue change percentage (month-over-month)
            // List is chronological: last element = current month, second-to-last = previous month
            var revenueChangePercent = 0.0;
            if (monthlyRevenue.Count >= 2)
            {
                var currentMonth = monthlyRevenue[^1].Revenue;
                var previousMonth = monthlyRevenue[^2].Revenue;
                revenueChangePercent = previousMonth > 0
                    ? ((currentMonth - previousMonth) / previousMonth) * 100
                    : 0;
            }

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
                ProductCoverageRate = productCoverageRate,
                RevenueChangePercent = Math.Round(revenueChangePercent, 2),
                MonthlyClients = monthlyClients,
            };
        }

        private async Task<double> GetRepurchaseClientsAsync(Guid userId)
        {
            var pipeline = _transactionsCollection.Aggregate()
                .Match(t => t.CreatedBy == userId)
                .Group(
                    t => t.ClientId,
                    g => new
                    {
                        ClientId = g.Key,
                        TransactionCount = g.Count()
                    }
                );

            var clientTransactionCounts = await pipeline.ToListAsync();
            var repurchasingClients = clientTransactionCounts.Count(c => c.TransactionCount >= 2);

            return (double)repurchasingClients; 
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
            var now = DateTime.UtcNow;

            // Get all transactions to compute cumulative revenue
            var filter = Builders<Transaction>.Filter.Eq(t => t.CreatedBy, userId);
            var transactions = await _transactionsCollection
                .Find(filter)
                .Project(t => new { t.Date, t.TotalPrice })
                .ToListAsync();

            // Build cumulative revenue for the last 5 months
            var result = new List<MonthlyRevenueDto>();
            for (int i = 4; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var endOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1).AddMonths(1);
                var cumulativeRevenue = transactions
                    .Where(t => t.Date < endOfMonth)
                    .Sum(t => t.TotalPrice);
                result.Add(new MonthlyRevenueDto
                {
                    Month = $"{targetMonth.Year}-{targetMonth.Month:00}",
                    Revenue = cumulativeRevenue
                });
            }

            return result;
        }

        private async Task<double> GetProductCoverageRateAsync(Guid userId)
        {
            // Count total products
            var productFilter = Builders<Product>.Filter.Eq(p => p.CreatedBy, userId);
            var totalProducts = await _productsCollection.CountDocumentsAsync(productFilter);

            if (totalProducts == 0) return 0;

            // Get distinct product IDs that appear in transactions
            var transactionFilter = Builders<Transaction>.Filter.Eq(t => t.CreatedBy, userId);
            var transactions = await _transactionsCollection
                .Find(transactionFilter)
                .Project(t => t.SoldProducts)
                .ToListAsync();

            var soldProductIds = transactions
                .SelectMany(sp => sp)
                .Select(sp => sp.ProductId)
                .Distinct()
                .Count();

            return (double)soldProductIds / totalProducts * 100;
        }

        private async Task<List<MonthlyClientsDto>> GetMonthlyClientsAsync(Guid userId)
        {
            // Get all clients created by this user (we need all to compute cumulative totals)
            var filter = Builders<Client>.Filter.Eq(c => c.CreatedBy, userId);
            var clients = await _clientsCollection
                .Find(filter)
                .Project(c => new { c.CreatedAt })
                .ToListAsync();

            // Build cumulative client count for the last 5 months
            var now = DateTime.UtcNow;
            var result = new List<MonthlyClientsDto>();

            for (int i = 4; i >= 0; i--)
            {
                var targetMonth = now.AddMonths(-i);
                var endOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1).AddMonths(1);
                var count = clients.Count(c => c.CreatedAt < endOfMonth);
                result.Add(new MonthlyClientsDto
                {
                    Month = $"{targetMonth.Year}-{targetMonth.Month:00}",
                    Count = count
                });
            }

            return result;
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
