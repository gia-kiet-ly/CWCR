using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnterpriseServiceAreaController : ControllerBase
    {
        private readonly IEnterpriseServiceAreaService _service;

        public EnterpriseServiceAreaController(
            IEnterpriseServiceAreaService service)
        {
            _service = service;
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> Create(
            CreateEnterpriseServiceAreaDto dto)
        {
            var userId = Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _service.CreateAsync(userId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<EnterpriseServiceAreaDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ================= GET ALL (FILTER + PAGING) =================
        [HttpGet]
        public async Task<ActionResult<PagedEnterpriseServiceAreaDto>> GetAll(
            [FromQuery] EnterpriseServiceAreaFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEnterpriseServiceAreaDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        // ================= DELETE (SOFT) =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}