using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Myapp.GeneralClass;
using MyApp.GeneralClass;

namespace Myapp.DashboardStats
{
    [ApiController]
    [Route("api/stats/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(DashboardService dashboardService, ILogger<DashboardController> logger) 
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }


        [Authorize(Roles = "user,admin")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("GET /api/dashboard/stats called");

                var userId = GetCurrentUserId();

                var stats = await _dashboardService.GetDashboardStatsAsync(userId);

                var response = new ApiResponse<DashboardStatsDto>(
                    success: true,
                    message: "Dashboard statistics retrieved successfully",
                    data: stats
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                var errorResponse = new ApiResponse<DashboardStatsDto>(
                    success: false,
                    message: $"An error occurred: {ex.Message}",
                    data: null
                );
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
        }

    }
}
