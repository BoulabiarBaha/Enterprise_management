using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Myapp.Settings;
using MyApp.GeneralClass;

namespace Myapp.Fournisseurs
{
    public class FournisseurService
    {
        private readonly IMongoCollection<Fournisseur> _fournisseurs;

        public FournisseurService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _fournisseurs = database.GetCollection<Fournisseur>("Fournisseurs");
        }

        // Get all fournisseurs (only for admin)
        public async Task<List<Fournisseur>> GetFournisseursAsync()
        {
            return await _fournisseurs.Find(f => true).ToListAsync();
        }

        // Get all user fournisseurs
        public async Task<List<Fournisseur>> GetMyFournisseursAsync(Guid userId)
        {
            return await _fournisseurs.Find(f => f.CreatedBy == userId).ToListAsync();
        }

        // Get a fournisseur by ID
        public async Task<Fournisseur> GetFournisseurByIdAsync(Guid id) =>
            await _fournisseurs.Find<Fournisseur>(f => f.Id == id).FirstOrDefaultAsync();

        // Create a new fournisseur
        public async Task<Fournisseur> CreateFournisseurAsync(FournisseurRequest request)
        {
            var fournisseur = new Fournisseur
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Tel = request.Tel,
                Address = request.Address,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _fournisseurs.InsertOneAsync(fournisseur);
            return fournisseur;
        }

        // Update a fournisseur
        public async Task UpdateFournisseurAsync(Guid id, Fournisseur fournisseur)
        {
            fournisseur.UpdatedAt = DateTime.UtcNow;
            await _fournisseurs.ReplaceOneAsync(f => f.Id == id, fournisseur);
        }

        // Delete a fournisseur
        public async Task DeleteFournisseurAsync(Guid id) =>
            await _fournisseurs.DeleteOneAsync(f => f.Id == id);

        // Map Fournisseur to FournisseurDTO
        public FournisseurDTO MapToDTO(Fournisseur fournisseur)
        {
            return new FournisseurDTO
            {
                Id = fournisseur.Id,
                Name = fournisseur.Name,
                Email = fournisseur.Email,
                Tel = fournisseur.Tel,
                Address = fournisseur.Address,
                CreatedBy = fournisseur.CreatedBy,
                CreatedAt = fournisseur.CreatedAt,
                UpdatedAt = fournisseur.UpdatedAt
            };
        }

        // Map a list of Fournisseurs to a list of FournisseurDTOs
        public List<FournisseurDTO> MapToListDTOs(List<Fournisseur> fournisseurs)
        {
            return fournisseurs.Select(f => MapToDTO(f)).ToList();
        }
    }
}
