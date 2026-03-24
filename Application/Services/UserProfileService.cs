using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly IImageService _imageService;

        public UserProfileService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow,
            IImageService imageService)
        {
            _userManager = userManager;
            _uow = uow;
            _imageService = imageService;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Ward)
                    .ThenInclude(w => w.District)
                .Include(u => u.District)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Unknown";

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                WardId = user.WardId,
                WardName = user.Ward?.Name,
                DistrictId = user.DistrictId,
                DistrictName = user.District?.Name ?? user.Ward?.District?.Name,
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Ward)
                    .ThenInclude(w => w.District)
                .Include(u => u.District)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            if (dto.WardId.HasValue || dto.DistrictId.HasValue)
            {
                await ValidateLocationAsync(dto.WardId, dto.DistrictId);
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                user.FullName = dto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            {
                user.PhoneNumber = dto.PhoneNumber;
            }

            user.WardId = dto.WardId;
            user.DistrictId = dto.DistrictId;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new KeyValuePair<string, ICollection<string>>(
                        e.Code,
                        new List<string> { e.Description }))
                    .ToList();

                throw new BaseException.ValidationException(errors);
            }

            user = await _userManager.Users
                .Include(u => u.Ward)
                    .ThenInclude(w => w.District)
                .Include(u => u.District)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var roles = await _userManager.GetRolesAsync(user!);
            var role = roles.FirstOrDefault() ?? "Unknown";

            return new UserProfileDto
            {
                Id = user!.Id,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                WardId = user.WardId,
                WardName = user.Ward?.Name,
                DistrictId = user.DistrictId,
                DistrictName = user.District?.Name ?? user.Ward?.District?.Name,
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<UserProfileDto> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BaseException.BadRequestException(
                    "invalid_file",
                    "Avatar file is required.");
            }

            var user = await _userManager.Users
                .Include(u => u.Ward)
                    .ThenInclude(w => w.District)
                .Include(u => u.District)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new BaseException.NotFoundException("user_not_found", "User not found.");
            }

            await using var stream = file.OpenReadStream();

            await _imageService.CheckSafeSearchAsync(stream);
            stream.Position = 0;

            var uploadResult = await _imageService.UploadImageAsync(stream, file.FileName);

            if (!string.IsNullOrWhiteSpace(user.AvatarPublicId))
            {
                await _imageService.DeleteImageAsync(user.AvatarPublicId);
            }

            user.AvatarUrl = uploadResult.Url;
            user.AvatarPublicId = uploadResult.PublicId;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

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
            var role = roles.FirstOrDefault() ?? "Unknown";

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                WardId = user.WardId,
                WardName = user.Ward?.Name,
                DistrictId = user.DistrictId,
                DistrictName = user.District?.Name ?? user.Ward?.District?.Name,
                Role = role,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        private async Task ValidateLocationAsync(Guid? wardId, Guid? districtId)
        {
            if (wardId.HasValue)
            {
                var wardRepo = _uow.GetRepository<Ward>();
                var ward = await wardRepo.GetByIdAsync(wardId.Value);

                if (ward == null)
                {
                    throw new BaseException.BadRequestException(
                        "invalid_ward",
                        $"Ward with ID {wardId} does not exist.");
                }

                if (districtId.HasValue && ward.DistrictId != districtId.Value)
                {
                    throw new BaseException.BadRequestException(
                        "ward_district_mismatch",
                        "The provided Ward does not belong to the provided District.");
                }
            }

            if (districtId.HasValue)
            {
                var districtRepo = _uow.GetRepository<District>();
                var district = await districtRepo.GetByIdAsync(districtId.Value);

                if (district == null)
                {
                    throw new BaseException.BadRequestException(
                        "invalid_district",
                        $"District with ID {districtId} does not exist.");
                }
            }
        }
    }
}