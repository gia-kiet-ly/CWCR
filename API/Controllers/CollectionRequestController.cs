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
    [Route("api/[controller]")]
    [Authorize] // yêu cầu JWT
    public class CollectionRequestController : ControllerBase
    {
        private readonly ICollectionRequestService _service;
        private readonly IUnitOfWork _uow;

        public CollectionRequestController(ICollectionRequestService service, IUnitOfWork uow)
        {
            _service = service;
            _uow = uow;
        }

        // =============================
        // ENTERPRISE INBOX (PAGED)
        // GET: /api/CollectionRequest/enterprise?Status=Offered&PageNumber=1&PageSize=10
        // =============================
        [HttpGet("Enterprise")]
        public async Task<ActionResult<PagedCollectionRequestDto>> GetEnterpriseInbox([FromQuery] CollectionRequestFilterDto filter)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            // default status = Offered nếu không truyền
            if (string.IsNullOrWhiteSpace(filter.Status))
                filter.Status = "Offered";

            var result = await _service.GetPagedForEnterpriseAsync(enterpriseId.Value, filter);
            return Ok(result);
        }

        // =============================
        // ENTERPRISE ACCEPT
        // POST: /api/CollectionRequest/accept
        // body: { "requestId": "..." }
        // =============================
        [HttpPost("accept")]
        public async Task<ActionResult> Accept([FromBody] AcceptCollectionRequestDto dto)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var ok = await _service.AcceptAsync(enterpriseId.Value, dto.RequestId);
            if (!ok) return BadRequest("Cannot accept request");

            return Ok(new { message = "Accepted" });
        }

        // =============================
        // ENTERPRISE REJECT
        // POST: /api/CollectionRequest/reject
        // body: { "requestId": "...", "reason": "..." }
        // =============================
        [HttpPost("reject")]
        public async Task<ActionResult> Reject([FromBody] RejectCollectionRequestDto dto)
        {
            var enterpriseId = await ResolveEnterpriseIdFromTokenAsync();
            if (enterpriseId == null) return Forbid();

            var ok = await _service.RejectAsync(enterpriseId.Value, dto.RequestId, dto.Reason);
            if (!ok) return BadRequest("Cannot reject request");

            return Ok(new { message = "Rejected" });
        }

        // =============================
        // Resolve enterpriseId from JWT userId -> lookup RecyclingEnterprise.UserId
        // =============================
        private async Task<Guid?> ResolveEnterpriseIdFromTokenAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr)) return null;
            if (!Guid.TryParse(userIdStr, out var userId)) return null;

            var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();

            var enterpriseId = await enterpriseRepo.NoTrackingEntities
                .Where(e => !e.IsDeleted && e.UserId == userId)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync();

            return enterpriseId;
        }
    }
}