using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _dashboardService.GetAdminDashboardAsync();
            return Ok(result);
        }

        [HttpGet("citizen")]
        [Authorize(Roles = "Citizen")]
        public async Task<IActionResult> GetCitizenDashboard()
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetCitizenDashboardAsync(userId);
            return Ok(result);
        }

        [HttpGet("enterprise")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetEnterpriseDashboard()
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetEnterpriseDashboardAsync(userId);
            return Ok(result);
        }

        [HttpGet("collector")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetCollectorDashboard()
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetCollectorDashboardAsync(userId);
            return Ok(result);
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }

            return Guid.Parse(userIdClaim);
        }
    }
}