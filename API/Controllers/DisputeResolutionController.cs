using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DisputeResolutionController : ControllerBase
    {
        private readonly IDisputeResolutionService _service;

        public DisputeResolutionController(
            IDisputeResolutionService service)
        {
            _service = service;
        }

        // =====================================================
        // CREATE (ADMIN)
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateDisputeResolutionDto dto)
        {
            var handlerId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.CreateAsync(handlerId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // =====================================================
        // GET BY COMPLAINT
        // =====================================================
        [HttpGet("by-complaint/{complaintId}")]
        public async Task<IActionResult> GetByComplaintId(Guid complaintId)
        {
            var result =
                await _service.GetByComplaintIdAsync(complaintId);

            return Ok(result);
        }

        // =====================================================
        // PAGING + FILTER
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DisputeResolutionFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}