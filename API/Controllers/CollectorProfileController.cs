using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/enterprise/collectors")]
    [Authorize(Roles = "Enterprise")]
    public class CollectorProfileController : ControllerBase
    {
        private readonly ICollectorProfileService _service;

        public CollectorProfileController(ICollectorProfileService service)
        {
            _service = service;
        }

        private Guid GetEnterpriseId()
        {
            return Guid.Parse(User.FindFirst("enterpriseId")!.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCollectorProfileDto dto)
        {
            var result = await _service.CreateAsync(GetEnterpriseId(), dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(GetEnterpriseId(), id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync(GetEnterpriseId());
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] CollectorProfileFilterDto filter)
        {
            var result = await _service.GetPagedAsync(GetEnterpriseId(), filter);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateCollectorProfileDto dto)
        {
            var ok = await _service.UpdateAsync(GetEnterpriseId(), id, dto);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(GetEnterpriseId(), id);
            return ok ? Ok() : NotFound();
        }
    }
}