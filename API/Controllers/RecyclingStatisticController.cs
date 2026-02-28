using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecyclingStatisticController : ControllerBase
    {
        private readonly IRecyclingStatisticService _service;

        public RecyclingStatisticController(
            IRecyclingStatisticService service)
        {
            _service = service;
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<ActionResult<RecyclingStatisticDto>> Create(
            [FromBody] CreateRecyclingStatisticDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<ActionResult<RecyclingStatisticDto>> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ================= GET ALL (FILTER + PAGING) =================
        [HttpGet]
        public async Task<ActionResult<PagedRecyclingStatisticDto>> GetAll(
            [FromQuery] RecyclingStatisticFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRecyclingStatisticDto dto)
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