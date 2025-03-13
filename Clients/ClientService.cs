using MongoDB.Driver;
using Myapp.Settings;
using Microsoft.Extensions.Options;
using Myapp.Models;


namespace Myapp.Clients
{
    public class ClientService
    {
        private readonly IMongoCollection<Client> _clients;

        public ClientService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _clients = database.GetCollection<Client>("Clients");
        }

        public async Task<List<Client>> GetClientsAsync() =>
            await _clients.Find(c => true).ToListAsync();

        public async Task<Client> GetClientAsync(Guid id) =>
            await _clients.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task CreateClientAsync(Client client) =>
            await _clients.InsertOneAsync(client);

        public async Task UpdateClientAsync(Guid id, Client client)
        {
            // Get the existing client
            var existingClient = await _clients.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingClient == null)
            {
                throw new Exception("Client not found.");
            }
            client.CreatedAt = existingClient.CreatedAt;
            client.UpdatedAt = DateTime.UtcNow;
            await _clients.ReplaceOneAsync(c => c.Id == id, client);
        }
            
        public async Task DeleteClientAsync(Guid id) =>
            await _clients.DeleteOneAsync(c => c.Id == id);
        
        public async Task UpdateClientPartialAsync(Guid id, UpdateClientDTO updateClientDTO)
        {
            // Récupérer le client existant
            var existingClient = await _clients.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (existingClient == null)
            {
                throw new Exception("Client not found.");
            }

            // Mettre à jour uniquement les champs fournis
            if (updateClientDTO.Name != null)
            {
                existingClient.Name = updateClientDTO.Name;
            }
            if (updateClientDTO.Email != null)
            {
                existingClient.Email = updateClientDTO.Email;
            }
            if (updateClientDTO.Tel != null)
            {
                existingClient.Tel = updateClientDTO.Tel;
            }
            if (updateClientDTO.Address != null)
            {
                existingClient.Address = updateClientDTO.Address;
            }

            // Mettre à jour UpdatedAt
            existingClient.UpdatedAt = DateTime.UtcNow;

            // Enregistrer les modifications
            await _clients.ReplaceOneAsync(c => c.Id == id, existingClient);
        }


        // Map Client to ClientDTO
        public ClientDTO MapToClientDTO(Client client)
        {
            var mappedClientDTO = new ClientDTO
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                NumIdentiteFiscal = client.NumIdentiteFiscal,
                Tel = client.Tel,
                Address = client.Address,
                Value = client.Value,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt
            };
            client.TransactionIds.ForEach( t => mappedClientDTO.TransactionIds.Add(t));
            client.BillingIds.ForEach (b => mappedClientDTO.BillingIds.Add(b));
            return mappedClientDTO;
        }
        
        // Map a list of Clients to a list of ClientDTOs
        public List<ClientDTO> MapToListDTOs(List<Client> clients)
        {
            return clients.Select(client => MapToClientDTO(client)).ToList();
        }
    }
}