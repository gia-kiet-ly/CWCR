using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WasteReportController : ControllerBase
    {
        private readonly IWasteReportService _service;

        public WasteReportController(IWasteReportService service)
        {
            _service = service;
        }

        // =========================================
        // CREATE
        // =========================================
        [HttpPost]
        public async Task<IActionResult> Create(CreateWasteReportDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(id);
        }

        // =========================================
        // UPDATE
        // =========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateWasteReportDto dto)
        {
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        // =========================================
        // DELETE
        // =========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        // =========================================
        // GET BY ID
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // =========================================
        // GET PAGED + FILTER
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] WasteReportFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}