using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Base;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = SystemRoles.Administrator)]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _service;

        public UserManagementController(IUserManagementService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all users with filtering and pagination
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paged list of users</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filter)
        {
            var data = await _service.GetAllUsersAsync(filter);

            var response = new BaseResponse<PagedUserDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Lấy danh sách người dùng thành công"
            );

            return Ok(response);
        }

        /// <summary>
        /// Update user active status
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Status update request</param>
        /// <returns>Updated status</returns>
        [HttpPut("{userId:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            Guid userId,
            [FromBody] UpdateUserStatusDto dto)
        {
            var data = await _service.UpdateUserStatusAsync(userId, dto);

            var response = new BaseResponse<UpdateUserStatusResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                data.Message
            );

            return Ok(response);
        }
    }
}
