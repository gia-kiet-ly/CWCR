using Application.Contract.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetProfileAsync(Guid userId);
        Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto);
        Task<UserProfileDto> UploadAvatarAsync(Guid userId, IFormFile file);
    }
}