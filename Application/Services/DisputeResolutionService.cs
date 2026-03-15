using Application.Constants;
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
        // ENTERPRISE - CREATE DISPUTE RESPONSE
        // =====================================================
        public async Task<DisputeResolutionResponseDto> CreateAsync(
            Guid enterpriseId,
            CreateDisputeResolutionDto dto)
        {
            var complaintRepo = _unitOfWork.GetRepository<Complaint>();
            var disputeRepo = _unitOfWork.GetRepository<DisputeResolution>();

            var complaint = await complaintRepo.GetByIdAsync(dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found.");

            if (complaint.Status != ComplaintStatus.InReview)
                throw new Exception("Complaint is not under review.");

            if (!complaint.CreatedBy.HasValue)
                throw new Exception("Complaint creator not found.");

            // Prevent enterprise responding multiple times
            var existed = disputeRepo.Entities.Any(x =>
                x.ComplaintId == dto.ComplaintId &&
                x.EnterpriseId == enterpriseId);

            if (existed)
                throw new Exception("Enterprise already responded to this complaint.");

            var now = DateTimeOffset.UtcNow;

            var dispute = new DisputeResolution
            {
                ComplaintId = dto.ComplaintId,
                EnterpriseId = enterpriseId,
                ResponseNote = dto.ResponseNote,
                ResolvedAt = now
            };

            await disputeRepo.InsertAsync(dispute);

            // Update complaint status
            complaint.Status = ComplaintStatus.EnterpriseResponded;
            await complaintRepo.UpdateAsync(complaint);

            await _unitOfWork.SaveAsync();

            // Notify citizen
            await _notificationService.CreateAsync(
                complaint.CreatedBy.Value,
                NotificationConstants.Types.DISPUTE_RESOLVED,
                complaint.Id.ToString()
            );

            return MapToDto(dispute);
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
        // FILTER + PAGING (ADMIN)
        // =====================================================
        public async Task<PagedDisputeResolutionDto>
            GetPagedAsync(DisputeResolutionFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<DisputeResolution>();

            var query = repo.Entities.AsQueryable();

            if (filter.ComplaintId.HasValue)
                query = query.Where(x =>
                    x.ComplaintId == filter.ComplaintId.Value);

            if (filter.EnterpriseId.HasValue)
                query = query.Where(x =>
                    x.EnterpriseId == filter.EnterpriseId.Value);

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
                EnterpriseId = entity.EnterpriseId,
                ResponseNote = entity.ResponseNote,
                ResolvedAt = entity.ResolvedAt,
                CreatedTime = entity.CreatedTime
            };
        }
    }
}