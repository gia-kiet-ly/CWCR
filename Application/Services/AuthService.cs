using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwt,
            IUnitOfWork uow,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwt = jwt;
            _uow = uow;
            _emailService = emailService;
            _configuration = configuration;
        }

        // ============================
        // REGISTER
        // ============================
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var existedUser = await _userManager.FindByEmailAsync(request.Email);
            if (existedUser != null)
            {
                throw new BaseException.BadRequestException(
                    "email_already_exists",
                    $"Email {request.Email} already exists.");
            }

            var allowedRoles = new[] { SystemRoles.Citizen, SystemRoles.RecyclingEnterprise };
            if (!allowedRoles.Contains(request.Role))
            {
                throw new BaseException.BadRequestException(
                    "invalid_role",
                    "Role is not allowed.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var errors = roleResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                throw new BaseException.BadRequestException(
                    "missing_frontend_base_url",
                    "Frontend:BaseUrl is missing in configuration.");
            }

            var verifyUrl =
                $"{frontendBaseUrl.TrimEnd('/')}/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2>Verify your email</h2>
                    <p>Hello {user.FullName},</p>
                    <p>Please verify your email by clicking the button below:</p>
                    <p>
                        <a href='{verifyUrl}'
                           style='display:inline-block;padding:10px 18px;background:#16a34a;color:#fff;text-decoration:none;border-radius:6px;'>
                           Verify Email
                        </a>
                    </p>
                    <p>If the button does not work, copy this link:</p>
                    <p>{verifyUrl}</p>
                </div>";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Verify your email",
                emailBody);

            return new RegisterResponseDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                Role = request.Role,
                CreatedAt = user.CreatedAt,
                RequireEmailVerification = true,
                Message = "Registration successful. Please verify your email."
            };
        }

        // ============================
        // VERIFY EMAIL
        // ============================
        public async Task<EmailVerificationResultDto> VerifyEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new BaseException.NotFoundException(
                    "user_not_found",
                    "User not found.");
            }

            if (user.EmailConfirmed)
            {
                var existingRoles = await _userManager.GetRolesAsync(user);
                var existingRole = existingRoles.FirstOrDefault() ?? string.Empty;

                var nextStep = "Login";

                if (existingRole == SystemRoles.RecyclingEnterprise)
                {
                    var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
                    var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                        x.UserId == user.Id && !x.IsDeleted);

                    nextStep = enterprise == null ? "CompleteEnterpriseProfile" : "WaitForApproval";
                }

                return new EmailVerificationResultDto
                {
                    Succeeded = true,
                    Email = user.Email ?? string.Empty,
                    Role = existingRole,
                    Message = "Email already verified.",
                    NextStep = nextStep
                };
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role == SystemRoles.Citizen)
            {
                return new EmailVerificationResultDto
                {
                    Succeeded = true,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    Message = "Email verified successfully.",
                    NextStep = "Login"
                };
            }

            if (role == SystemRoles.RecyclingEnterprise)
            {
                var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
                var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && !x.IsDeleted);

                var nextStep = enterprise == null
                    ? "CompleteEnterpriseProfile"
                    : "WaitForApproval";

                return new EmailVerificationResultDto
                {
                    Succeeded = true,
                    Email = user.Email ?? string.Empty,
                    Role = role,
                    Message = "Email verified successfully.",
                    NextStep = nextStep
                };
            }

            return new EmailVerificationResultDto
            {
                Succeeded = true,
                Email = user.Email ?? string.Empty,
                Role = role,
                Message = "Email verified successfully.",
                NextStep = "Login"
            };
        }

        // ============================
        // LOGIN
        // ============================
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect.");
            }

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_credentials",
                    "Email or password is incorrect.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            await EnsureUserCanLoginAsync(user, role);

            var (accessToken, expiredAt) = _jwt.GenerateToken(user.Id, user.Email!, role);

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

            string? enterpriseApprovalStatus = null;
            bool mustCompleteEnterpriseProfile = false;
            bool canLogin = true;

            if (role == SystemRoles.RecyclingEnterprise)
            {
                var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
                var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && !x.IsDeleted);

                enterpriseApprovalStatus = enterprise?.ApprovalStatus.ToString();
                mustCompleteEnterpriseProfile = enterprise == null;

                // Enterprise vẫn login được để bổ sung hồ sơ,
                // nhưng chỉ "dùng hệ thống bình thường" khi đã approved
                canLogin = enterprise != null &&
                           enterprise.ApprovalStatus == Core.Enum.EnterpriseApprovalStatus.Approved;
            }

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiredAt = expiredAt,

                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Role = role,

                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,

                EnterpriseApprovalStatus = enterpriseApprovalStatus,
                CanLogin = canLogin,
                MustCompleteEnterpriseProfile = mustCompleteEnterpriseProfile
            };
        }

        // ============================
        // REFRESH TOKEN
        // ============================
        public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request)
        {
            var refreshRepo = _uow.GetRepository<RefreshToken>();

            var hashedToken = RefreshTokenHasher.Hash(request.RefreshToken);

            var storedToken = await refreshRepo.FirstOrDefaultAsync(r => r.TokenHash == hashedToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.Expires <= DateTime.UtcNow)
            {
                throw new BaseException.UnauthorizedException(
                    "invalid_refresh_token",
                    "Refresh token is invalid or expired.");
            }

            storedToken.IsRevoked = true;
            refreshRepo.Update(storedToken);

            var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                throw new BaseException.UnauthorizedException(
                    "user_not_found",
                    "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            await EnsureUserCanLoginAsync(user, role);

            var (newAccessToken, expiredAt) = _jwt.GenerateToken(user.Id, user.Email!, role);

            var rawRefreshToken = RefreshTokenGenerator.Generate();
            var refreshTokenHash = RefreshTokenHasher.Hash(rawRefreshToken);

            var newRefreshEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                TokenHash = refreshTokenHash,
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            await refreshRepo.InsertAsync(newRefreshEntity);
            await _uow.SaveAsync();

            string? enterpriseApprovalStatus = null;
            bool mustCompleteEnterpriseProfile = false;
            bool canLogin = true;

            if (role == SystemRoles.RecyclingEnterprise)
            {
                var enterpriseRepo = _uow.GetRepository<RecyclingEnterprise>();
                var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && !x.IsDeleted);

                enterpriseApprovalStatus = enterprise?.ApprovalStatus.ToString();
                mustCompleteEnterpriseProfile = enterprise == null;
                canLogin = enterprise != null &&
                           enterprise.ApprovalStatus == Core.Enum.EnterpriseApprovalStatus.Approved;
            }

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = rawRefreshToken,
                ExpiredAt = expiredAt,

                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Role = role,

                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,

                EnterpriseApprovalStatus = enterpriseApprovalStatus,
                CanLogin = canLogin,
                MustCompleteEnterpriseProfile = mustCompleteEnterpriseProfile
            };
        }

        // ============================
        // PRIVATE
        // ============================
        private async Task EnsureUserCanLoginAsync(ApplicationUser user, string role)
        {
            if (!user.EmailConfirmed)
            {
                throw new BaseException.UnauthorizedException(
                    "email_not_confirmed",
                    "Please verify your email before logging in.");
            }

            if (!user.IsActive)
            {
                throw new BaseException.UnauthorizedException(
                    "account_inactive",
                    "Your account is inactive.");
            }

            // Citizen và Enterprise đều cho login sau khi verify email
            if (role == SystemRoles.Citizen || role == SystemRoles.RecyclingEnterprise)
            {
                return;
            }
        }
    }
}