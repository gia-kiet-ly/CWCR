using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Domain.Entities;
using Infrastructure.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CollectorService : ICollectorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CollectorService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<CreateCollectorResponseDto> CreateCollectorAsync(
    Guid enterpriseUserId,
    CreateCollectorDto dto)
        {
            await EnsureEnterpriseUserAsync(enterpriseUserId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var collectorProfileRepo = _unitOfWork.GetRepository<CollectorProfile>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId == enterpriseUserId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_profile_not_found",
                    "Enterprise profile not found.");
            }

            var email = dto.Email.Trim();

            var existedUser = await _userManager.FindByEmailAsync(email);
            if (existedUser != null)
            {
                throw new BaseException.BadRequestException(
                    "email_already_exists",
                    $"Email {email} already exists.");
            }

            var collectorUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = string.IsNullOrWhiteSpace(dto.FullName) ? null : dto.FullName.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(collectorUser, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(collectorUser, SystemRoles.Collector);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(collectorUser);

                var errors = roleResult.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            try
            {
                var collectorProfile = new CollectorProfile
                {
                    CollectorId = collectorUser.Id,
                    EnterpriseId = enterprise.Id,
                    IsActive = true,
                    IsProfileCompleted = false
                };

                await collectorProfileRepo.InsertAsync(collectorProfile);
                await _unitOfWork.SaveAsync();

                return new CreateCollectorResponseDto
                {
                    UserId = collectorUser.Id,
                    CollectorProfileId = collectorProfile.Id,
                    EnterpriseId = enterprise.Id,
                    Email = collectorUser.Email ?? string.Empty,
                    FullName = collectorUser.FullName,
                    IsActive = collectorUser.IsActive,
                    IsProfileCompleted = collectorProfile.IsProfileCompleted,
                    Role = SystemRoles.Collector
                };
            }
            catch
            {
                await _userManager.RemoveFromRoleAsync(collectorUser, SystemRoles.Collector);
                await _userManager.DeleteAsync(collectorUser);
                throw;
            }
        }

        public async Task<List<CollectorItemDto>> GetMyCollectorsAsync(Guid enterpriseUserId)
        {
            await EnsureEnterpriseUserAsync(enterpriseUserId);

            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var collectorProfileRepo = _unitOfWork.GetRepository<CollectorProfile>();

            var enterprise = await enterpriseRepo.FirstOrDefaultAsync(x =>
                x.UserId == enterpriseUserId && !x.IsDeleted);

            if (enterprise == null)
            {
                throw new BaseException.NotFoundException(
                    "enterprise_profile_not_found",
                    "Enterprise profile not found.");
            }

            var collectors = await collectorProfileRepo.Entities
                .Include(x => x.Collector)
                .Where(x =>
                    x.EnterpriseId == enterprise.Id &&
                    !x.IsDeleted &&
                    !x.Collector.IsDeleted)
                .OrderByDescending(x => x.CreatedTime)
                .ToListAsync();

            return collectors.Select(x => new CollectorItemDto
            {
                UserId = x.CollectorId,
                CollectorProfileId = x.Id,
                EnterpriseId = x.EnterpriseId,
                Email = x.Collector.Email ?? string.Empty,
                FullName = x.Collector.FullName,
                IsActive = x.IsActive && x.Collector.IsActive,
                IsProfileCompleted = x.IsProfileCompleted
            }).ToList();
        }

        private async Task<ApplicationUser> EnsureEnterpriseUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                throw new BaseException.NotFoundException(
                    "user_not_found",
                    "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            if (role != SystemRoles.RecyclingEnterprise)
            {
                throw new BaseException.UnauthorizedException(
                    "enterprise_role_required",
                    "Only enterprise accounts can perform this action.");
            }

            if (!user.EmailConfirmed)
            {
                throw new BaseException.UnauthorizedException(
                    "email_not_confirmed",
                    "Please verify your email before continuing.");
            }

            if (!user.IsActive)
            {
                throw new BaseException.UnauthorizedException(
                    "account_inactive",
                    "Your account is inactive.");
            }

            return user;
        }
    }
}
