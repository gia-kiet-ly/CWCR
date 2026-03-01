using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Core.Utils;
using Domain.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
                "Đăng ký thành công"
            );

            return StatusCode(201, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _auth.LoginAsync(request);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var result = await _auth.RefreshAsync(request.RefreshToken);
            return Ok(result);
        }
    }
}
