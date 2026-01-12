using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.GeneralClass;
using MyApp.GeneralClass;

namespace Myapp.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/users
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            try{
                _logger.LogInformation("GET /api/users called");
                var users = await _userService.GetUsersAsync();
                var userDTOs = _userService.MapToListDTOs(users);

                var response = new ApiResponse<List<UserDTO>>(
                        success: true,
                        message: "Users retrieved successfully",
                        data: userDTOs
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
        
        // GET: api/users/{id}
        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetUser(Guid id)
        {
            try{
                _logger.LogInformation("GET /api/users/id called");
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null){
                    var badResponse = new ApiResponse<User>(
                    success: false,
                    message: "User not found, verify the ID please",
                    data: user
                );
                return BadRequest(badResponse);
                }

                var mappedUser = _userService.MapToDTO(user);
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "User retrieved successfully",
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

        // POST: api/users
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDTO>>> CreateUser([FromBody] User user)
        {
            try{
                _logger.LogInformation("POST /api/users called");
                user.Id = Guid.NewGuid(); // Generate a new GUID for the user
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                await _userService.CreateUserAsync(user);

                var mappedUser = _userService.MapToDTO(user);
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "new user is created",
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

        // PUT: api/users/{id}
        [Authorize(Roles = "admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUser(Guid id, User user)
        {
            try{
                if (id != user.Id){
                    var notFound = new ApiResponse<User>(
                        success: false,
                        message: "User ID mismatch.",
                        data: null
                    );
                    return BadRequest(notFound);
                }
                await _userService.UpdateUserAsync(id, user);
                var mappedUser = _userService.MapToDTO(user);
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "User updated successfully",
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

        // DELETE: api/users/{id}
        [Authorize(Roles = "admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> DeleteUser(Guid id)
        {

            try{
                var usr = await _userService.GetUserByIdAsync(id);

                if (usr == null){
                    var notFound = new ApiResponse<User>(
                        success: false,
                        message: "User not found",
                        data: null
                    );
                    return NotFound(notFound);
                }
                await _userService.DeleteUserAsync(id);
                
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "User deleted successfully",
                    data: null
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


        // GET: api/users/me
        [Authorize(Roles = "user,admin")]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetCurrentUser()
        {
            try
            {
                _logger.LogInformation("GET /api/users/me called");
                var userId = GetCurrentUserId();

                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    var notFoundResponse = new ApiResponse<UserDTO>(
                        success: false,
                        message: "User not found",
                        data: null
                    );
                    return NotFound(notFoundResponse);
                }


                var mappedUser = _userService.MapToDTO(user);
                var response = new ApiResponse<UserDTO>(
                    success: true,
                    message: "User retrieved successfully",
                    data: mappedUser
                );
                return Ok(response);

            }

            catch (UnauthorizedAccessException ex)
            {
                var unauthorizedResponse = new ApiResponse<UserDTO>(
                    success: false,
                    message: ex.Message,
                    data: null
                );
                return Unauthorized(unauthorizedResponse);
            }
            catch (Exception ex)
            {

                var errorResponse = new ApiResponse<User>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }

        }




    }
}