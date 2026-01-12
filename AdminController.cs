using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Authentification;
using MyApp.GeneralClass;

namespace Myapp.Admin
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AdminController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("generate-n8n-token")]
        public ActionResult GenerateN8nToken([FromBody] GenerateServiceTokenRequest request)
        {
            var token = _jwtService.GenerateServiceToken(request.ServiceName, request.ExpiryInDays);

            var response = new ApiResponse<string>(
                success: true,
                message: "Service token generated successfully.",
                data: token
            );

            return Ok(response);
        }
    }
}


