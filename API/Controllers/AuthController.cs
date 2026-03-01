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

            var message = request.Role == Infrastructure.Repo.SystemRoles.RecyclingEnterprise
                ? "Đăng ký thành công, vui lòng chờ admin phê duyệt"
                : "Đăng ký thành công";

            var response = new BaseResponse<RegisterResponseDto>(
                StatusCodeHelper.Created,
                StatusCodeHelper.Created.Name(),
                data,
                message
            );

            return StatusCode(StatusCodes.Status201Created, response);
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
            var data = await _auth.RefreshAsync(request.RefreshToken);

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