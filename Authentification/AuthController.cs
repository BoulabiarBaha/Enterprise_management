using Microsoft.AspNetCore.Mvc;
using Myapp.Users;
using MyApp.Authentification;
using MyApp.GeneralClass;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginRequest request)
        {
            // Validate user credentials (e.g., check username and password)
            var user = await _userService.GetUserByUsernameAsync(request.Username);
            var verif = BCrypt.Net.BCrypt.Verify(request.Password,user.PasswordHash);
            if (user == null || !verif) 
            {
                var errorResponse = new ApiResponse<string>(
                    success: false,
                    message: "Invalid username or password",
                    data: null
                );

                return Unauthorized(errorResponse);
            }

            // Generate JWT
            var token = _jwtService.GenerateToken(user.Id.ToString(), user.Username, user.Role );

            var response = new ApiResponse<string>(
                success: true,
                message: "Login successful",
                data: token
            );

            return Ok(response);
        }

        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Signup([FromBody] SignupRequest request){
            try{
                // Validate required fields
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    var missingResponse = new ApiResponse<User>(
                        success: false,
                        message: "Username, Email, and Password fields are required.",
                        data: null
                    );
                    return BadRequest(missingResponse);
                }
                // Validate email format
                if (!IsValidEmail(request.Email))
                {
                    var invalidEmailResponse = new ApiResponse<User>(
                        success: false,
                        message: "Invalid email format.",
                        data: null
                    );
                    return BadRequest(invalidEmailResponse);
                }
                // Check if the user already exists
                var existingUser = await _userService.GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    var duplicateUserResponse = new ApiResponse<User>(
                        success: false,
                        message: "A user with this email already exists.",
                        data: null
                    );
                    return Conflict(duplicateUserResponse);
                }
                var user = new User{
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    Username = request.Username,
                    Phone = request.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "user"
                };
                await _userService.CreateUserAsync(user);
                var mappedUser = _userService.MapToDTO(user);
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "You account has been created successfully",
                    data: mappedUser
                );
                return Ok(response);
            }
            catch(Exception ex){
                var errorResponse = new ApiResponse<User>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }
        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try{
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch{  return false;  }
        }
    }

    
}