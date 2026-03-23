using Application.Contract.DTOs;

namespace Application.Contract.Services
{
    public interface IComplaintService
    {
        // =============================
        // CITIZEN
        // =============================

        Task<ComplaintResponseDto> CreateAsync(
            Guid complainantId,
            CreateComplaintDto dto);

        Task<PagedComplaintDto> GetMyComplaintsAsync(
            Guid complainantId,
            ComplaintFilterDto filter);

        Task<ComplaintResponseDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            bool isAdmin);

        Task<bool> DeleteAsync(
            Guid id,
            Guid complainantId);

        // =============================
        // ADMIN
        // =============================

        Task<PagedComplaintDto> GetAllAsync(
            ComplaintFilterDto filter);

        Task<ComplaintResponseDto> UpdateStatusAsync(
            Guid id,
            UpdateComplaintStatusDto dto,
            Guid adminId);

        Task<PagedComplaintDto> GetEnterpriseComplaintsAsync(
    Guid enterpriseId,
    ComplaintFilterDto filter);
    }
}