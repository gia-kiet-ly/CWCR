using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/collector-assignments")]
    [Authorize]
    public class CollectorAssignmentController : ControllerBase
    {
        private readonly ICollectorAssignmentService _service;
        private readonly IUnitOfWork _uow;

        public CollectorAssignmentController(ICollectorAssignmentService service, IUnitOfWork uow)
        {
            _service = service;
            _uow = uow;
        }

        private Guid GetUserId()
            => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<Guid?> ResolveEnterpriseIdFromTokenAsync()
        {
            var userId = GetUserId();

            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();

            return await enterpriseRepo.NoTrackingEntities
                .Where(e => !e.IsDeleted && e.UserId == userId)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync();
        }

        // =========================================================
        // ENTERPRISE: CREATE ASSIGNMENT
        // POST /api/collector-assignments
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.CreateAsync(enterpriseId.Value, dto);
            return Ok(result);
        }

        // =========================================================
        // ENTERPRISE: GET BY ID
        // GET /api/collector-assignments/enterprise/{id}
        // =========================================================
        [HttpGet("enterprise/{id:guid}")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetByIdEnterprise(Guid id)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetByIdEnterpriseAsync(enterpriseId.Value, id);
            return result == null ? NotFound() : Ok(result);
        }

        // =========================================================
        // ENTERPRISE: GET PAGED
        // GET /api/collector-assignments/enterprise?pageNumber=1&pageSize=10
        // =========================================================
        [HttpGet("enterprise")]
        [Authorize(Roles = "Enterprise")]
        public async Task<IActionResult> GetPagedEnterprise([FromQuery] AssignmentFilterDto filter)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetPagedEnterpriseAsync(enterpriseId.Value, filter);
            return Ok(result);
        }

        // =========================================================
        // COLLECTOR: GET BY ID
        // GET /api/collector-assignments/collector/{id}
        // =========================================================
        [HttpGet("collector/{id:guid}")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetByIdCollector(Guid id)
        {
            var result = await _service.GetByIdCollectorAsync(GetUserId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        // =========================================================
        // COLLECTOR: GET PAGED
        // GET /api/collector-assignments/collector?pageNumber=1&pageSize=10
        // =========================================================
        [HttpGet("collector")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetPagedCollector([FromQuery] AssignmentFilterDto filter)
        {
            var result = await _service.GetPagedCollectorAsync(GetUserId(), filter);
            return Ok(result);
        }

        // =========================================================
        // COLLECTOR: UPDATE STATUS
        // PUT /api/collector-assignments/collector/{id}/status
        // =========================================================
        [HttpPut("collector/{id:guid}/status")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAssignmentStatusDto dto)
        {
            var ok = await _service.UpdateStatusCollectorAsync(GetUserId(), id, dto);
            return ok ? Ok() : BadRequest();
        }
    }
}