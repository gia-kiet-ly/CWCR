using Application.Contract.DTOs;

namespace Application.Contract.Interfaces.Services
{
    public interface IDisputeResolutionService
    {
        // =============================
        // ADMIN - CREATE RESOLUTION
        // =============================
        Task<DisputeResolutionResponseDto> CreateAsync(
            Guid adminId,
            CreateDisputeResolutionDto dto);

        // =============================
        // GET BY ID
        // =============================
        Task<DisputeResolutionResponseDto?> GetByIdAsync(Guid id);

        // =============================
        // GET BY COMPLAINT
        // =============================
        Task<List<DisputeResolutionResponseDto>>
            GetByComplaintIdAsync(Guid complaintId);

        // =============================
        // GET ALL (ADMIN - FILTER + PAGING)
        // =============================
        Task<PagedDisputeResolutionDto> GetPagedAsync(
            DisputeResolutionFilterDto filter);
    }
}