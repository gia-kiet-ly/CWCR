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
    [Route("api/recycling-enterprises")]
    [Authorize(Roles = SystemRoles.RecyclingEnterprise)]
    public class RecyclingEnterpriseController : ControllerBase
    {
        private readonly IRecyclingEnterpriseService _service;

        public RecyclingEnterpriseController(IRecyclingEnterpriseService service)
        {
            _service = service;
        }

        [HttpPost("me/profile")]
        public async Task<IActionResult> CreateOrUpdateProfile(
            [FromBody] CreateOrUpdateEnterpriseProfileRequestDto dto)
        {
            var userId = GetCurrentUserId();

            var data = await _service.CreateOrUpdateProfileAsync(userId, dto);

            var response = new BaseResponse<EnterpriseProfileResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Cập nhật hồ sơ doanh nghiệp thành công"
            );

            return Ok(response);
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            var data = await _service.GetMyEnterpriseProfileAsync(userId);

            var response = new BaseResponse<EnterpriseProfileResponseDto?>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Lấy hồ sơ doanh nghiệp thành công"
            );

            return Ok(response);
        }

        [HttpPost("me/documents")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(
            [FromForm] UploadEnterpriseDocumentRequestDto dto)
        {
            var userId = GetCurrentUserId();

            var data = await _service.UploadDocumentAsync(userId, dto);

            var response = new BaseResponse<EnterpriseDocumentResponseDto>(
                StatusCodeHelper.Created,
                StatusCodeHelper.Created.Name(),
                data,
                "Upload tài liệu thành công"
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpGet("me/documents")]
        public async Task<IActionResult> GetMyDocuments()
        {
            var userId = GetCurrentUserId();

            var data = await _service.GetMyDocumentsAsync(userId);

            var response = new BaseResponse<List<EnterpriseDocumentResponseDto>>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Lấy danh sách tài liệu thành công"
            );

            return Ok(response);
        }

        [HttpPut("me/environment-license")]
        public async Task<IActionResult> SetEnvironmentLicense(
            [FromBody] SetEnvironmentLicenseRequestDto dto)
        {
            var userId = GetCurrentUserId();

            var data = await _service.SetEnvironmentLicenseAsync(userId, dto);

            var response = new BaseResponse<EnterpriseProfileResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Cập nhật giấy phép môi trường thành công"
            );

            return Ok(response);
        }

        [HttpPost("me/submit")]
        public async Task<IActionResult> SubmitProfile(
            [FromBody] SubmitEnterpriseProfileRequestDto dto)
        {
            var userId = GetCurrentUserId();

            var data = await _service.SubmitProfileAsync(userId, dto);

            var response = new BaseResponse<SubmitEnterpriseProfileResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Gửi hồ sơ doanh nghiệp thành công"
            );

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new BaseException.UnauthorizedException(
                    "user_id_not_found",
                    "Không tìm thấy thông tin người dùng trong token.");
            }

            return Guid.Parse(userIdClaim);
        }
    }
}