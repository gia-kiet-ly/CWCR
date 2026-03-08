using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/collectors")]
    [ApiController]
    [Authorize(Roles = SystemRoles.RecyclingEnterprise)]
    public class CollectorController : ControllerBase
    {
        private readonly ICollectorService _collectorService;

        public CollectorController(ICollectorService collectorService)
        {
            _collectorService = collectorService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollector([FromBody] CreateCollectorDto dto)
        {
            var enterpriseUserId = GetCurrentUserId();

            var result = await _collectorService.CreateCollectorAsync(enterpriseUserId, dto);

            return Ok(BaseResponse<CreateCollectorResponseDto>.OkResponse(
                result,
                "Collector account created successfully."));
        }

        [HttpGet("my-collectors")]
        public async Task<IActionResult> GetMyCollectors()
        {
            var enterpriseUserId = GetCurrentUserId();

            var result = await _collectorService.GetMyCollectorsAsync(enterpriseUserId);

            return Ok(BaseResponse<List<CollectorItemDto>>.OkResponse(
                result,
                "Collectors retrieved successfully."));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_user_claim",
                    "User id claim is missing or invalid.");
            }

            return userId;
        }
    }
}
