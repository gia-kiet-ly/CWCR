using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WasteReportController : ControllerBase
    {
        private readonly IWasteReportService _service;

        public WasteReportController(IWasteReportService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWasteReportDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
                return Unauthorized();

            var citizenId = Guid.Parse(userIdValue);

            var result = await _service.CreateAsync(dto, citizenId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWasteReportDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required.");

            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] WasteReportFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);
            return Ok(result);
        }

        [HttpGet("{reportId:guid}/proof")]
        public async Task<IActionResult> GetProof(Guid reportId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
                return Unauthorized();

            var citizenId = Guid.Parse(userIdValue);

            var result = await _service.GetProofForCitizenAsync(reportId, citizenId);

            if (result == null)
                return NotFound("Proof not uploaded yet");

            return Ok(result);
        }
        [HttpPost("{id:guid}/redispatch")]
        public async Task<IActionResult> Redispatch(Guid id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
                return Unauthorized();

            await _service.RedispatchAsync(id);

            return Ok("Redispatch triggered.");
        }

        [HttpGet("{reportId:guid}/reject-history")]
        public async Task<IActionResult> GetRejectHistory(Guid reportId)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
                return Unauthorized();

            var citizenId = Guid.Parse(userIdValue);

            var result = await _service.GetRejectHistoryAsync(reportId, citizenId);

            return Ok(result);
        }
    }
}