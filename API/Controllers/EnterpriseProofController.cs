using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/enterprise/proofs")]
    [Authorize(Roles = "RecyclingEnterprise")]
    public class EnterpriseProofController : ControllerBase
    {
        private readonly ICollectionProofService _service;

        public EnterpriseProofController(ICollectionProofService service)
        {
            _service = service;
        }

        private Guid GetEnterpriseId() => Guid.Parse(User.FindFirst("enterpriseId")!.Value);
        private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdEnterpriseAsync(GetEnterpriseId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] CollectionProofFilterDto filter)
        {
            var result = await _service.GetPagedEnterpriseAsync(GetEnterpriseId(), filter);
            return Ok(result);
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> Review(Guid id, ReviewCollectionProofDto dto)
        {
            var ok = await _service.ReviewAsync(GetEnterpriseId(), GetUserId(), id, dto);
            return ok ? Ok() : BadRequest();
        }
    }
}
