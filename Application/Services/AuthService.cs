using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IUnitOfWork _uow;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwt,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _jwt = jwt;
            _uow = uow;
        }

        // ============================
        // REGISTER
        // ============================
        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var existedUser = await _userManager.FindByEmailAsync(request.Email);
            if (existedUser != null)
                throw new BaseException.BadRequestException(
                    "email_already_exists",
                    $"Email {request.Email} already exists."
                );

            // ✅ Validate role whitelist (không cho register Admin)
            var allowedRoles = new[]
            {
                "Citizen",
                "Collector",
                "Recycling Enterprise"
            };

            if (!allowedRoles.Contains(request.Role))
                throw new BaseException.BadRequestException(
                    "invalid_role",
                    "Role is not allowed."
                );

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }
                    ))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            // ✅ Gán role cho user
            await _userManager.AddToRoleAsync(user, request.Role);
        }

        // ============================
        // LOGIN
        // ============================
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1️⃣ Kiểm tra user tồn tại
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect."
                );

            // 2️⃣ Kiểm tra password
            var valid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!valid)
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect."
                );

            // ✅ Lấy role của user
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            // ✅ Generate Access Token có role
            var (accessToken, expiredAt) =
                _jwt.GenerateToken(user.Id, user.Email!, role);

            // Generate refresh token (RAW)
            var rawRefreshToken = RefreshTokenGenerator.Generate();

            // 5️⃣ Hash refresh token trước khi lưu DB
            var refreshTokenHash = RefreshTokenHasher.Hash(rawRefreshToken);

            var refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = refreshTokenHash,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            // 6️⃣ Lưu DB
            var refreshRepo = _uow.GetRepository<RefreshToken>();
            await refreshRepo.InsertAsync(refreshEntity);
            await _uow.SaveAsync();

            // 7️⃣ Trả raw refresh token về client
            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiredAt = expiredAt
            };
        }

        // ============================
        // REFRESH TOKEN
        // ============================
        public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
        {
            var refreshRepo = _uow.GetRepository<RefreshToken>();

            var hashedToken = RefreshTokenHasher.Hash(refreshToken);

            var storedToken = await refreshRepo.FirstOrDefaultAsync(
                r => r.TokenHash == hashedToken
            );

            if (storedToken == null ||
                storedToken.IsRevoked ||
                storedToken.Expires <= DateTime.UtcNow)
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_refresh_token",
                    "Refresh token is invalid or expired."
                );
            }

            // revoke token cũ
            storedToken.IsRevoked = true;
            refreshRepo.Update(storedToken);

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

            if (user == null)
                throw new BaseException.UnauthorizedException(
                    "user_not_found",
                    "User not found."
                );

            // ✅ Lấy role lại
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            var (newAccessToken, expiredAt) =
                _jwt.GenerateToken(user.Id, user.Email!, role);

            var rawRefreshToken = RefreshTokenGenerator.Generate();
            var refreshTokenHash = RefreshTokenHasher.Hash(rawRefreshToken);

            var refreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = refreshTokenHash,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            await refreshRepo.InsertAsync(refreshEntity);
            await _uow.SaveAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = rawRefreshToken,
                ExpiredAt = expiredAt
            };
        }
    }
}
