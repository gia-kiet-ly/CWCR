using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictService _service;

        public DistrictController(IDistrictService service)
        {
            _service = service;
        }

        [HttpGet("by-province/{provinceCode}")]
        public async Task<IActionResult> GetByProvince(string provinceCode)
        {
            var result = await _service.GetByProvinceAsync(provinceCode);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDistrictDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateDistrictDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] DistrictFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}