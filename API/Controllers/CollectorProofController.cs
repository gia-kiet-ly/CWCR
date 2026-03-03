using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/collector-proofs")]
    public class CollectorProofController : ControllerBase
    {
        private readonly ICollectionProofService _service;

        public CollectorProofController(ICollectionProofService service)
        {
            _service = service;
        }

        private Guid GetUserId()
            => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private Guid GetEnterpriseId()
            => Guid.Parse(User.FindFirst("enterpriseId")!.Value);

        // =====================================================
        // COLLECTOR: CREATE PROOF
        // POST /api/collector-proofs
        // =====================================================
        [HttpPost]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> Create([FromBody] CreateCollectionProofDto dto)
        {
            var result = await _service.CreateAsync(GetUserId(), dto);
            return Ok(result);
        }

        // =====================================================
        // COLLECTOR: GET BY ID
        // GET /api/collector-proofs/collector/{id}
        // =====================================================
        [HttpGet("collector/{id:guid}")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetByIdCollector(Guid id)
        {
            var result = await _service.GetByIdCollectorAsync(GetUserId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        // =====================================================
        // COLLECTOR: GET PAGED
        // GET /api/collector-proofs/collector
        // =====================================================
        [HttpGet("collector")]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> GetPagedCollector([FromQuery] CollectionProofFilterDto filter)
        {
            var result = await _service.GetPagedCollectorAsync(GetUserId(), filter);
            return Ok(result);
        }

        // =====================================================
        // ENTERPRISE: GET BY ID
        // GET /api/collector-proofs/enterprise/{id}
        // =====================================================
        [HttpGet("enterprise/{id:guid}")]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> GetByIdEnterprise(Guid id)
        {
            var result = await _service.GetByIdEnterpriseAsync(GetEnterpriseId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        // =====================================================
        // ENTERPRISE: GET PAGED
        // GET /api/collector-proofs/enterprise
        // =====================================================
        [HttpGet("enterprise")]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> GetPagedEnterprise([FromQuery] CollectionProofFilterDto filter)
        {
            var result = await _service.GetPagedEnterpriseAsync(GetEnterpriseId(), filter);
            return Ok(result);
        }

        // =====================================================
        // ENTERPRISE: REVIEW PROOF
        // PUT /api/collector-proofs/enterprise/{id}/review
        // =====================================================
        [HttpPut("enterprise/{id:guid}/review")]
        [Authorize(Roles = "RecyclingEnterprise")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewCollectionProofDto dto)
        {
            var ok = await _service.ReviewAsync(GetEnterpriseId(), GetUserId(), id, dto);
            return ok ? Ok() : BadRequest();
        }
    }
}