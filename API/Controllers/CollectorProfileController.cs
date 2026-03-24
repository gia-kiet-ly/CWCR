using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/enterprise/collectors")]
    [Authorize]
    public class CollectorProfileController : ControllerBase
    {
        private readonly ICollectorProfileService _service;

        public CollectorProfileController(ICollectorProfileService service)
        {
            _service = service;
        }

        private Guid GetEnterpriseId()
        {
            var enterpriseIdClaim = User.FindFirst("enterpriseId")?.Value;

            if (string.IsNullOrWhiteSpace(enterpriseIdClaim) || !Guid.TryParse(enterpriseIdClaim, out var enterpriseId))
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_token",
                    "Invalid or missing enterprise ID in token.");
            }

            return enterpriseId;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_token",
                    "Invalid or missing user ID in token.");
            }

            return userId;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetMyCollectorProfile()
        {
            var result = await _service.GetMyCollectorProfileAsync(GetCurrentUserId());
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> Create(CreateCollectorProfileDto dto)
        {
            var result = await _service.CreateAsync(GetEnterpriseId(), dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(GetEnterpriseId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetEnterpriseId());
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetPaged([FromQuery] CollectorProfileFilterDto filter)
        {
            var result = await _service.GetPagedAsync(GetEnterpriseId(), filter);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> Update(Guid id, UpdateCollectorProfileDto dto)
        {
            var ok = await _service.UpdateAsync(GetEnterpriseId(), id, dto);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(GetEnterpriseId(), id);
            return ok ? Ok() : NotFound();
        }
    }
}