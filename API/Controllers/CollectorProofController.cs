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
    [Route("api/collector-proofs")]
    [Authorize]
    public class CollectorProofController : ControllerBase
    {
        private readonly ICollectionProofService _service;
        private readonly IUnitOfWork _uow;

        public CollectorProofController(ICollectionProofService service, IUnitOfWork uow)
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
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetByIdEnterpriseAsync(enterpriseId.Value, id);
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
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetPagedEnterpriseAsync(enterpriseId.Value, filter);
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
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var ok = await _service.ReviewAsync(enterpriseId.Value, GetUserId(), id, dto);
            return ok ? Ok() : BadRequest();
        }
    }
}