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
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existedUser = await _userManager.FindByEmailAsync(request.Email);
            if (existedUser != null)
                throw new BaseException.BadRequestException(
                    "email_already_exists",
                    $"Email {request.Email} already exists."
                );

            // ✅ only 2 roles
            var allowedRoles = new[] { "Citizen", "Enterprise" };
            if (!allowedRoles.Contains(request.Role))
                throw new BaseException.BadRequestException(
                    "invalid_role",
                    "Role is not allowed."
                );

            // ✅ Enterprise: require enterpriseInfo + validate tax code length
            if (request.Role == "Enterprise")
            {
                if (request.EnterpriseInfo == null)
                    throw new BaseException.BadRequestException(
                        "enterprise_info_required",
                        "EnterpriseInfo is required for Enterprise registration."
                    );

                var tax = request.EnterpriseInfo.TaxCode?.Trim() ?? "";
                if (tax.Length < 10 || tax.Length > 13)
                    throw new BaseException.BadRequestException(
                        "invalid_tax_code",
                        "TaxCode must be 10 to 13 characters."
                    );
            }

            // ✅ BE override IsActive: Citizen active now, Enterprise pending
            var isActive = request.Role == "Citizen";

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.Phone,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }
                    ))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            // ✅ Add role; if fail -> rollback user
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user); // rollback

                var errors = roleResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }
                    ))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            // ✅ If Enterprise: create profile Pending; if fail -> rollback user
            if (request.Role == "Enterprise")
            {
                try
                {
                    var info = request.EnterpriseInfo!;

                    var enterpriseRepo = _uow.GetRepository<EnterpriseProfile>();

                    var profile = new EnterpriseProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,

                        EnterpriseName = info.EnterpriseName,
                        TaxCode = info.TaxCode,
                        Address = info.Address,
                        LegalRepresentative = info.LegalRepresentative,
                        RepresentativePosition = info.RepresentativePosition,
                        EnvironmentLicenseFileId = info.EnvironmentLicenseFileId,

                        Status = Core.Enum.EnterpriseApprovalStatus.PendingApproval,
                        CreatedAt = DateTime.UtcNow
                    };

                    await enterpriseRepo.InsertAsync(profile);
                    await _uow.SaveAsync();
                }
                catch
                {
                    await _userManager.DeleteAsync(user); // rollback
                    throw;
                }
            }

            return new RegisterResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? "",
                Role = request.Role,
                CreatedAt = user.CreatedAt
            };
        }

        // ============================
        // LOGIN
        // ============================
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect."
                );

            // ✅ check password first
            var valid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!valid)
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect."
                );

            // ✅ then check active (avoid leaking status)
            if (!user.IsActive)
                throw new BaseException.UnauthorizedException(
                    "account_pending",
                    "Account is pending admin approval."
                );

            // ✅ get role
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            // ✅ token
            var (accessToken, expiredAt) =
                _jwt.GenerateToken(user.Id, user.Email!, role);

            // refresh token raw
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

            var refreshRepo = _uow.GetRepository<RefreshToken>();
            await refreshRepo.InsertAsync(refreshEntity);
            await _uow.SaveAsync();

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

            // revoke old
            storedToken.IsRevoked = true;
            refreshRepo.Update(storedToken);

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

            if (user == null)
                throw new BaseException.UnauthorizedException(
                    "user_not_found",
                    "User not found."
                );

            // ✅ optional: also block refresh if not active
            // if (!user.IsActive) throw new BaseException.UnauthorizedException("account_pending", "Account is pending admin approval.");

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