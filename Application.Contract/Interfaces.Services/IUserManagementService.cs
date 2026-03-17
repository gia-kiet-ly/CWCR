using Application.Contract.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.Interfaces.Services
{
    public interface IUserManagementService
    {
        /// <summary>
        /// Admin: Get all users with filtering and pagination
        /// </summary>
        Task<PagedUserDto> GetAllUsersAsync(UserFilterDto filter);

        /// <summary>
        /// Admin: Update user active status
        /// </summary>
        Task<UpdateUserStatusResponseDto> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto);
    }
}
