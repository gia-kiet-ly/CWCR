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
    [Route("api/admin/enterprise-approvals")]
    [Authorize(Roles = SystemRoles.Administrator)]
    public class EnterpriseApprovalController : ControllerBase
    {
        private readonly IEnterpriseApprovalService _service;

        public EnterpriseApprovalController(IEnterpriseApprovalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetEnterprises([FromQuery] RecyclingEnterpriseFilterDto filter)
        {
            var data = await _service.GetEnterprisesAsync(filter);

            var response = new BaseResponse<PagedRecyclingEnterpriseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Lấy danh sách doanh nghiệp thành công"
            );

            return Ok(response);
        }

        [HttpGet("{enterpriseId:guid}")]
        public async Task<IActionResult> GetEnterpriseDetail(Guid enterpriseId)
        {
            var data = await _service.GetEnterpriseApprovalDetailAsync(enterpriseId);

            if (data == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_not_found",
                    "Enterprise profile not found.");
            }

            var response = new BaseResponse<EnterpriseApprovalDetailDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Lấy chi tiết doanh nghiệp thành công"
            );

            return Ok(response);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveEnterpriseRequestDto dto)
        {
            var reviewerId = GetCurrentUserId();

            var data = await _service.ApproveAsync(dto, reviewerId);

            var response = new BaseResponse<EnterpriseApprovalResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Phê duyệt doanh nghiệp thành công"
            );

            return Ok(response);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> Reject([FromBody] RejectEnterpriseRequestDto dto)
        {
            var reviewerId = GetCurrentUserId();

            var data = await _service.RejectAsync(dto, reviewerId);

            var response = new BaseResponse<EnterpriseApprovalResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Từ chối doanh nghiệp thành công"
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