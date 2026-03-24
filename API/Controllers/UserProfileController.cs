using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Base;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/user-profile")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;

        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var data = await _userProfileService.GetProfileAsync(userId);

            var response = new BaseResponse<UserProfileDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Profile retrieved successfully."
            );

            return Ok(response);
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDto dto)
        {
            var userId = GetCurrentUserId();
            var data = await _userProfileService.UpdateProfileAsync(userId, dto);

            var response = new BaseResponse<UserProfileDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Profile updated successfully."
            );

            return Ok(response);
        }

        /// <summary>
        /// Upload current user's avatar
        /// </summary>
        [HttpPost("me/avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMyAvatar([FromForm] UploadAvatarRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var data = await _userProfileService.UploadAvatarAsync(userId, dto.File);

            var response = new BaseResponse<UserProfileDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Avatar uploaded successfully."
            );

            return Ok(response);
        }

        /// <summary>
        /// Get user profile by ID (Admin only)
        /// </summary>
        [HttpGet("{userId:guid}")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> GetUserProfile([FromRoute] Guid userId)
        {
            var data = await _userProfileService.GetProfileAsync(userId);

            var response = new BaseResponse<UserProfileDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Profile retrieved successfully."
            );

            return Ok(response);
        }

        /// <summary>
        /// Update user profile by ID (Admin only)
        /// </summary>
        [HttpPut("{userId:guid}")]
        [Authorize(Roles = SystemRoles.Administrator)]
        public async Task<IActionResult> UpdateUserProfile(
            [FromRoute] Guid userId,
            [FromBody] UpdateUserProfileDto dto)
        {
            var data = await _userProfileService.UpdateProfileAsync(userId, dto);

            var response = new BaseResponse<UserProfileDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Profile updated successfully."
            );

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_token",
                    "Invalid or missing user ID in token.");
            }

            return userId;
        }
    }
}