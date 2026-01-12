using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Myapp.GeneralClass
{
    public class BaseController : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            // Case 1: normal user JWT (from frontend)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid jwtUserId))
            {
                return jwtUserId;
            }

            // Case 2: delegated user (from n8n)
            if (Request.Headers.TryGetValue("X-Acting-User-Id", out var delegatedUserId))
            {
                if (Guid.TryParse(delegatedUserId, out Guid actingUserId))
                {
                    return actingUserId;
                }
            }

            // No user context → reject
            throw new UnauthorizedAccessException("Acting user ID not found");
        }

        protected string? GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
