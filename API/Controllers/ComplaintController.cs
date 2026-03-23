using Application.Contract.DTOs;
using Application.Contract.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;

        public ComplaintController(IComplaintService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Enterprise")]
        [HttpGet("enterprise")]
        public async Task<IActionResult> GetEnterpriseComplaints(
    [FromQuery] ComplaintFilterDto filter)
        {
            var enterpriseId = GetUserId();

            var result = await _service
                .GetEnterpriseComplaintsAsync(enterpriseId, filter);

            return Ok(result);
        }
        // =====================================================
        // CREATE (Citizen)
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateComplaintDto dto)
        {
            var userId = GetUserId();

            var result = await _service.CreateAsync(userId, dto);

            return CreatedAtAction(nameof(GetById),
                new { id = result.Id },
                result);
        }

        // =====================================================
        // GET MY COMPLAINTS (Citizen)
        // =====================================================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyComplaints(
            [FromQuery] ComplaintFilterDto filter)
        {
            var userId = GetUserId();

            var result = await _service.GetMyComplaintsAsync(userId, filter);

            return Ok(result);
        }

        // =====================================================
        // GET ALL (Admin)
        // =====================================================
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] ComplaintFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        // =====================================================
        // GET BY ID (Citizen or Admin)
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            var isAdmin = User.IsInRole("Admin");

            var result = await _service.GetByIdAsync(id, userId, isAdmin);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // =====================================================
        // UPDATE STATUS (Admin)
        // =====================================================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateComplaintStatusDto dto)
        {
            var adminId = GetUserId();

            var result = await _service.UpdateStatusAsync(id, dto, adminId);

            return Ok(result);
        }

        // =====================================================
        // DELETE (Citizen only delete own Open complaint)
        // =====================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();

            var deleted = await _service.DeleteAsync(id, userId);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        // =====================================================
        // HELPER
        // =====================================================
        private Guid GetUserId()
        {
            return Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}