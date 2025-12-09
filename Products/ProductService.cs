using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Myapp.Billings;
using Myapp.Settings;
using Myapp.Users;
using MyApp.GeneralClass;
using MyApp.Products;

namespace MyApp.Products
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _products = database.GetCollection<Product>("Products");
        }

        // Get all products (only for admin)
        public async Task<List<Product>> GetProductsAsync()
        {
            return await _products.Find(p => true).ToListAsync();

        }

        // Get all user products
        public async Task<List<Product>> GetMyProductsAsync(Guid userId)
        {
            return await _products.Find(p => p.CreatedBy == userId).ToListAsync();

        }
           

        // Get a product by ID
        public async Task<Product> GetProductByIdAsync(Guid id) =>
            await _products.Find<Product>(product => product.Id == id).FirstOrDefaultAsync();


        // Create a new product
        public async Task<Product> CreateProductAsync(ProductRequest request)
        {
            var product = new Product{
                Id = Guid.NewGuid(),
                Name = request.Name,
                UnitPrice = request.UnitPrice,
                Description = request.Description,
                Supplier = request.Supplier,
                CreatedBy = request.CreatedBy
            };
            product.PriceHistory.Add(new PriceChange{
                Price = product.UnitPrice,
                Date = DateTime.UtcNow
            });
            await _products.InsertOneAsync(product);
            return product;
        }             

        // Update a product
        public async Task UpdateProductAsync(Guid id, Product product)
        {
            // Get the existing product
            var existingProduct = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
     
            // copy the priceHistory
            foreach(var item in existingProduct.PriceHistory) 
            {
                product.PriceHistory.Add(item);
            }
            // Check if the unitPrice has changed
            if (existingProduct.UnitPrice != product.UnitPrice)
            {   
                if (existingProduct.UnitPrice != existingProduct.PriceHistory.Last().Price)
                {
                    product.PriceHistory.Add(new PriceChange
                    {
                        Price = existingProduct.UnitPrice,
                        Date = DateTime.UtcNow
                    });
                }

            }
            // Update the product
            await _products.ReplaceOneAsync(p => p.Id == id, product);
        }

        // Delete a product
        public async Task DeleteProductAsync(Guid id) =>
            await _products.DeleteOneAsync(product => product.Id == id);

        // Map Product to ProductDTO
        public ProductDTO MapToDTO(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                UnitPrice = product.UnitPrice,
                PriceHistory = product.PriceHistory.Select(p => new PriceChangeDTO
                {
                    Price = p.Price,
                    Date = p.Date
                }).ToList(),
                Description = product.Description,
                Supplier = product.Supplier,
                CreatedBy = product.CreatedBy,
            };
        }

        // Map a list of Products to a list of ProductDTOs
        public List<ProductDTO> MapToListDTOs(List<Product> products)
        {
            return products.Select(product => MapToDTO(product)).ToList();
        }
    }
}