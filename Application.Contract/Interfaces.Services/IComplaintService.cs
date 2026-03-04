using Application.Contract.DTOs;

namespace Application.Contract.Services
{
    public interface IComplaintService
    {
        // =============================
        // CITIZEN
        // =============================

        Task<ComplaintResponseDto> CreateAsync(
            Guid currentUserId,
            CreateComplaintDto dto);

        Task<PagedComplaintDto> GetMyComplaintsAsync(
            Guid currentUserId,
            ComplaintFilterDto filter);

        Task<ComplaintResponseDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            bool isAdmin);

        Task<bool> DeleteAsync(
            Guid id,
            Guid currentUserId);

        // =============================
        // ADMIN
        // =============================

        Task<PagedComplaintDto> GetAllAsync(
            ComplaintFilterDto filter);

        Task<ComplaintResponseDto> UpdateStatusAsync(
            Guid id,
            UpdateComplaintStatusDto dto,
            Guid adminId);
    }
}