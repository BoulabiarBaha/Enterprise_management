using Microsoft.AspNetCore.Identity;
using MyApp.GeneralClass;

namespace Myapp.Users
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Username { get; set; } 
        public required string Email { get; set; } 
        public required string PasswordHash { get; set; }
        //public UserRole Role { get; set; } = UserRole.User;
        private string _role = "user";
        public string Role
        {
            get => _role;
            set
            {
                if (value != "user" && value != "admin")
                {
                    throw new ArgumentException("Role must be either 'user' or 'admin'.");
                }
                _role = value;
            }
        }
    }

    public class UserDTO
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
    }

}