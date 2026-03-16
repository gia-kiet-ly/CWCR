using Application.Contract.DTOs;
using Application.Contract.Interfaces.Services;
using Domain.Base;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PagedUserDto> GetAllUsersAsync(UserFilterDto filter)
        {
            var query = _userManager.Users.AsQueryable();

            // Filter by IsDeleted
            query = query.Where(u => !u.IsDeleted);

            // Filter by IsActive
            if (filter.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            }

            // Filter by EmailConfirmed
            if (filter.EmailConfirmed.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);
            }

            // Filter by SearchTerm (Email hoặc FullName)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(u =>
                    u.Email != null && u.Email.ToLower().Contains(searchTerm) ||
                    u.FullName != null && u.FullName.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            // Pagination
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = new List<UserItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Unknown";

                // Filter by Role nếu được chỉ định
                if (!string.IsNullOrWhiteSpace(filter.Role) &&
                    !role.Equals(filter.Role, StringComparison.OrdinalIgnoreCase))
                {
                    totalCount--; // Giảm total count nếu không match role
                    continue;
                }

                items.Add(new UserItemDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Role = role,
                    IsActive = user.IsActive,
                    IsDeleted = user.IsDeleted,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                });
            }

            return new PagedUserDto
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<UpdateUserStatusResponseDto> UpdateUserStatusAsync(
            Guid userId,
            UpdateUserStatusDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null || user.IsDeleted)
            {
                throw new BaseException.NotFoundException(
                    "user_not_found",
                    $"User with ID {userId} not found.");
            }

            user.IsActive = dto.IsActive;
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

            return new UpdateUserStatusResponseDto
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Message = dto.IsActive
                    ? "User has been activated successfully."
                    : "User has been deactivated successfully."
            };
        }
    }
}
