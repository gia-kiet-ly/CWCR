using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/collector-assignments")]
    public class CollectorAssignmentController : ControllerBase
    {
        private readonly ICollectorAssignmentService _service;

        public CollectorAssignmentController(ICollectorAssignmentService service)
        {
            _service = service;
        }

        private Guid GetUserId()
            => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private Guid GetEnterpriseId()
            => Guid.Parse(User.FindFirst("enterpriseId")!.Value);

        // =========================================================
        // ENTERPRISE: CREATE ASSIGNMENT
        // POST /api/collector-assignments
        // =========================================================
        [HttpPost]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentDto dto)
        {
            var result = await _service.CreateAsync(GetEnterpriseId(), dto);
            return Ok(result);
        }

        // =========================================================
        // ENTERPRISE: GET BY ID
        // GET /api/collector-assignments/enterprise/{id}
        // =========================================================
        [HttpGet("enterprise/{id:guid}")]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> GetByIdEnterprise(Guid id)
        {
            var result = await _service.GetByIdEnterpriseAsync(GetEnterpriseId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        // =========================================================
        // ENTERPRISE: GET PAGED
        // GET /api/collector-assignments/enterprise?pageNumber=1&pageSize=10
        // =========================================================
        [HttpGet("enterprise")]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> GetPagedEnterprise([FromQuery] AssignmentFilterDto filter)
        {
            var result = await _service.GetPagedEnterpriseAsync(GetEnterpriseId(), filter);
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