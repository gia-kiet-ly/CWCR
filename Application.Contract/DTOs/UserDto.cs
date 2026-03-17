using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Contract.DTOs
{
    // ================= GET ALL USERS =================
    public class UserFilterDto
    {
        public string? Role { get; set; } // Admin, Citizen, Enterprise, Collector
        public string? SearchTerm { get; set; } // Search by Email, FullName
        public bool? IsActive { get; set; }
        public bool? EmailConfirmed { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserItemDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PagedUserDto
    {
        public List<UserItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // ================= UPDATE USER STATUS =================
    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class UpdateUserStatusResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
