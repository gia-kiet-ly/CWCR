using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecyclingEnterpriseController : ControllerBase
    {
        private readonly IRecyclingEnterpriseService _service;

        public RecyclingEnterpriseController(
            IRecyclingEnterpriseService service)
        {
            _service = service;
        }

        // ================================
        // CREATE
        // ================================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateRecyclingEnterpriseDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // ================================
        // GET BY ID
        // ================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ================================
        // GET ALL WITH FILTER
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] RecyclingEnterpriseFilterDto filter)
        {
            var result = await _service.GetAllAsync(filter);
            return Ok(result);
        }

        // ================================
        // UPDATE
        // ================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateRecyclingEnterpriseDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // ================================
        // UPDATE STATUS (ADMIN)
        // ================================
        [HttpPatch("{id}/status")]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateEnterpriseStatusDto dto)
        {
            var success = await _service.UpdateStatusAsync(id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        // ================================
        // DELETE
        // ================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}