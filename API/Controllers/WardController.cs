using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WardController : ControllerBase
    {
        private readonly IWardService _service;

        public WardController(IWardService service)
        {
            _service = service;
        }
        [HttpGet("by-district/{districtId}")]
        public async Task<IActionResult> GetByDistrict(Guid districtId)
        {
            var result = await _service.GetByDistrictAsync(districtId);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateWardDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateWardDto dto)
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
        public async Task<IActionResult> GetPaged([FromQuery] WardFilterDto filter)
        {
            var result = await _service.GetPagedAsync(filter);
            return Ok(result);
        }
    }
}