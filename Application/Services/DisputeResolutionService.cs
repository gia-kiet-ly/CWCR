using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;

namespace Application.Services
{
    public class DisputeResolutionService : IDisputeResolutionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public DisputeResolutionService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        // =====================================================
        // CREATE RESOLUTION
        // =====================================================
        public async Task<DisputeResolutionResponseDto> CreateAsync(
            Guid handlerId,
            CreateDisputeResolutionDto dto)
        {
            var complaintRepo = _unitOfWork.GetRepository<Complaint>();
            var resolutionRepo = _unitOfWork.GetRepository<DisputeResolution>();

            var complaint = await complaintRepo.GetByIdAsync(dto.ComplaintId);
            if (complaint == null)
                throw new Exception("Complaint not found.");

            if (complaint.Status == ComplaintStatus.Resolved ||
                complaint.Status == ComplaintStatus.Rejected)
                throw new Exception("Complaint already closed.");

            var resolution = new DisputeResolution
            {
                ComplaintId = dto.ComplaintId,
                HandlerId = handlerId,
                ResolutionNote = dto.ResolutionNote,
                ResolvedAt = DateTimeOffset.UtcNow
            };

            await resolutionRepo.InsertAsync(resolution);

            complaint.Status = ComplaintStatus.Resolved;
            await complaintRepo.UpdateAsync(complaint);

            await _unitOfWork.SaveAsync();

            // ================= Notification =================
            await _notificationService.CreateAsync(
                complaint.CreatedBy.Value,
                "Dispute resolved",
                "Your complaint has been reviewed and resolved by the administrator.",
                complaint.Id
            );

            return MapToDto(resolution);
        }

        // =====================================================
        // GET BY ID
        // =====================================================
        public async Task<DisputeResolutionResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<DisputeResolution>();
            var entity = await repo.GetByIdAsync(id);

            return entity == null ? null : MapToDto(entity);
        }

        // =====================================================
        // GET BY COMPLAINT
        // =====================================================
        public async Task<List<DisputeResolutionResponseDto>>
            GetByComplaintIdAsync(Guid complaintId)
        {
            var repo = _unitOfWork.GetRepository<DisputeResolution>();

            var list = repo.Entities
                           .Where(x => x.ComplaintId == complaintId)
                           .OrderByDescending(x => x.ResolvedAt)
                           .ToList();

            return list.Select(MapToDto).ToList();
        }

        // =====================================================
        // FILTER + PAGING
        // =====================================================
        public async Task<PagedDisputeResolutionDto>
            GetPagedAsync(DisputeResolutionFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<DisputeResolution>();

            var query = repo.Entities.AsQueryable();

            if (filter.ComplaintId.HasValue)
                query = query.Where(x =>
                    x.ComplaintId == filter.ComplaintId.Value);

            if (filter.HandlerId.HasValue)
                query = query.Where(x =>
                    x.HandlerId == filter.HandlerId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(x =>
                    x.ResolvedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x =>
                    x.ResolvedAt <= filter.ToDate.Value);

            var paged = await repo.GetPagging(
                query.OrderByDescending(x => x.ResolvedAt),
                filter.PageNumber,
                filter.PageSize);

            return new PagedDisputeResolutionDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        // =====================================================
        // MAPPER
        // =====================================================
        private static DisputeResolutionResponseDto MapToDto(
            DisputeResolution entity)
        {
            return new DisputeResolutionResponseDto
            {
                Id = entity.Id,
                ComplaintId = entity.ComplaintId,
                HandlerId = entity.HandlerId,
                ResolutionNote = entity.ResolutionNote,
                ResolvedAt = entity.ResolvedAt,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}