using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PointRuleController : ControllerBase
    {
        private readonly IPointRuleService _service;

        public PointRuleController(IPointRuleService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all point rules
        /// Admin và Enterprise đều có thể xem
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Enterprise")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Create point rule (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreatePointRuleRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Update rule of a WasteType (Admin only)
        /// </summary>
        [HttpPut("{wasteTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid wasteTypeId, [FromBody] UpdatePointRuleRequest request)
        {
            var result = await _service.UpdateAsync(wasteTypeId, request);
            return Ok(result);
        }
    }
}
