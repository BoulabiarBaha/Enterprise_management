using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Myapp.GeneralClass
{
    public class BaseController : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            return userId;
        }

        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
