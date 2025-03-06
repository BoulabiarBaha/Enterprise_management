using MongoDB.Driver;
using Myapp.Settings;
using Microsoft.Extensions.Options;

namespace Myapp.Users
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
        {
            var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("Users");
        }

        public UserDTO MapToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
        public List<UserDTO> MapToListDTOs(List<User> users)
        {   
            var userDTOs = new List<UserDTO>();
            users.ForEach(e => { 
                var userDTO = new UserDTO{ 
                    Id = e.Id,
                    Username = e.Username,
                    Email = e.Email,
                    Role = e.Role
                };
                userDTOs.Add(userDTO);
            });
            return userDTOs;
        }

        // Get all users
        public async Task<List<User>> GetUsersAsync() =>
            await _users.Find(user => true).ToListAsync();

        // Get a user by ID
        public async Task<User> GetUserByIdAsync(Guid id) =>
            await _users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();

        // Create a new user
        public async Task CreateUserAsync(User user) =>
            await _users.InsertOneAsync(user);

        // Get a user by username
        public async Task<User> GetUserByUsernameAsync(string? username) =>
            await _users.Find<User>(user => user.Username == username).FirstOrDefaultAsync();
        
        // Get a user by Email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
        }
        
        // Update a user
        public async Task UpdateUserAsync(Guid id, User user)
        {
            // Get the existing user
            var existingUser = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }
            //update the user
            await _users.ReplaceOneAsync(user => user.Id == id, user);
        }
            

        // Delete a user
        public async Task DeleteUserAsync(Guid id) 
        {
            // Get the existing user
            var existingUser = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (existingUser == null)
            {
                throw new Exception("User not found.");
            }
            //Delete the user
            await _users.DeleteOneAsync(user => user.Id == id);
        }
            
    }
}