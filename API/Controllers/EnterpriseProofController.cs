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
    [Route("api/enterprise/proofs")]
    [Authorize(Roles = "Enterprise")]
    public class EnterpriseProofController : ControllerBase
    {
        private readonly ICollectionProofService _service;
        private readonly IUnitOfWork _uow;

        public EnterpriseProofController(ICollectionProofService service, IUnitOfWork uow)
        {
            _service = service;
            _uow = uow;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<Guid?> ResolveEnterpriseIdFromTokenAsync()
        {
            var userId = GetUserId();

            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();

            return await enterpriseRepo.NoTrackingEntities
                .Where(e => !e.IsDeleted && e.UserId == userId)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetByIdEnterpriseAsync(enterpriseId.Value, id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] CollectionProofFilterDto filter)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var result = await _service.GetPagedEnterpriseAsync(enterpriseId.Value, filter);
            return Ok(result);
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(Guid id, [FromBody] ReviewCollectionProofDto dto)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var ok = await _service.ReviewAsync(enterpriseId.Value, GetUserId(), id, dto);
            return ok ? Ok() : BadRequest();
        }
    }
}