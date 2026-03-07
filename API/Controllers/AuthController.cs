using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var data = await _auth.RegisterAsync(request);

            var response = new BaseResponse<RegisterResponseDto>(
                StatusCodeHelper.Created,
                StatusCodeHelper.Created.Name(),
                data,
                "Đăng ký thành công, vui lòng kiểm tra email để xác thực tài khoản."
            );

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [AllowAnonymous]
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            var data = await _auth.VerifyEmailAsync(userId, token);

            var response = new BaseResponse<EmailVerificationResultDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Xác thực email thành công."
            );

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var data = await _auth.LoginAsync(request);

            var response = new BaseResponse<AuthResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Đăng nhập thành công"
            );

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var data = await _auth.RefreshAsync(request);

            var response = new BaseResponse<AuthResponseDto>(
                StatusCodeHelper.OK,
                StatusCodeHelper.OK.Name(),
                data,
                "Refresh token thành công"
            );

            return Ok(response);
        }
    }
}