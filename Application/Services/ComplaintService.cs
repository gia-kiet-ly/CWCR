using Application.Contract.DTOs;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Application.Contract.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICitizenPointService _citizenPointService;

        public ComplaintService(IUnitOfWork unitOfWork, ICitizenPointService citizenPointService)
        {
            _unitOfWork = unitOfWork;
            _citizenPointService = citizenPointService;
        }
        // ================= CREATE =================
        public async Task<ComplaintResponseDto> CreateAsync(
            Guid complainantId,
            CreateComplaintDto dto)
        {
            if (!Enum.TryParse<ComplaintType>(dto.Type, true, out var type))
                throw new Exception("Invalid complaint type");

            var complaintRepo = _unitOfWork.GetRepository<Complaint>();
            var reportRepo = _unitOfWork.GetRepository<WasteReport>();
            var requestRepo = _unitOfWork.GetRepository<CollectionRequest>();

            // 1. Validate report
            var report = await reportRepo.GetByIdAsync(dto.ReportId);
            if (report == null || report.IsDeleted)
                throw new Exception("Report not found");

            // 2. Resolve CollectionRequest từ WasteReport
            var request = await requestRepo.NoTrackingEntities
                .Include(x => x.WasteReportWaste)
                .Where(x =>
                    x.WasteReportWaste.WasteReportId == dto.ReportId &&
                    !x.IsDeleted &&
                    (
                        x.Status == CollectionRequestStatus.Rejected ||
                        x.Status == CollectionRequestStatus.Completed
                    )
                )
                .OrderByDescending(x => x.CreatedTime)
                .FirstOrDefaultAsync();

            if (request == null)
                throw new Exception("No active collection request found for this report");

            // 3. Create complaint
            var complaint = new Complaint
            {
                ComplainantId = complainantId,
                ReportId = dto.ReportId,
                CollectionRequestId = request.Id, // 🔥 auto resolve
                Type = type,
                Status = ComplaintStatus.Open,
                Content = dto.Content
            };

            await complaintRepo.InsertAsync(complaint);
            await _unitOfWork.SaveAsync();

            return MapToDto(complaint);
        }

        // ================= GET MY =================
        public async Task<PagedComplaintDto> GetMyComplaintsAsync(
            Guid complainantId,
            ComplaintFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<Complaint>();

            var query = repo.NoTrackingEntities
                .Where(x => x.ComplainantId == complainantId && !x.IsDeleted);

            query = ApplyFilter(query, filter);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedComplaintDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items.Select(MapToDto).ToList()
            };
        }

        // ================= ADMIN - GET ALL =================
        public async Task<PagedComplaintDto> GetAllAsync(
            ComplaintFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<Complaint>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            query = ApplyFilter(query, filter);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedComplaintDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items.Select(MapToDto).ToList()
            };
        }

        // ================= GET BY ID =================
        // ================= GET BY ID =================
        public async Task<ComplaintResponseDto?> GetByIdAsync(
            Guid id,
            Guid currentUserId,
            bool isAdmin)
        {
            var repo = _unitOfWork.GetRepository<Complaint>();

            var complaint = await repo.NoTrackingEntities
                .Include(x => x.Resolutions)
                .Include(x => x.CollectionRequest) // 🔥 cần include để check EnterpriseId
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (complaint == null)
                return null;

            // Admin xem được tất cả
            if (isAdmin)
                return MapToDto(complaint);

            // Citizen xem được complaint của mình
            if (complaint.ComplainantId == currentUserId)
                return MapToDto(complaint);

            // Enterprise xem được complaint liên quan đến mình
            if (complaint.CollectionRequestId != null)
            {
                var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
                var enterprise = await enterpriseRepo.NoTrackingEntities
                    .FirstOrDefaultAsync(e => e.UserId == currentUserId && !e.IsDeleted);

                if (enterprise != null &&
                    complaint.CollectionRequest!.EnterpriseId == enterprise.Id)
                {
                    return MapToDto(complaint);
                }
            }

            throw new Exception("Unauthorized");
        }
        public async Task<PagedComplaintDto> GetEnterpriseComplaintsAsync(
         Guid userId,  // đây thực ra là UserId, không phải EnterpriseId
         ComplaintFilterDto filter)
        {
            var enterpriseRepo = _unitOfWork.GetRepository<RecyclingEnterprise>();
            var enterprise = await enterpriseRepo.NoTrackingEntities
                .FirstOrDefaultAsync(e => e.UserId == userId && !e.IsDeleted);

            if (enterprise == null)
                throw new Exception("Enterprise not found");

            var repo = _unitOfWork.GetRepository<Complaint>();

            var query = repo.NoTrackingEntities
                .Include(x => x.CollectionRequest)
                .Where(x =>
                    !x.IsDeleted &&
                    x.CollectionRequestId != null &&
                    x.CollectionRequest!.EnterpriseId == enterprise.Id);  // dùng enterprise.Id thật

            query = ApplyFilter(query, filter);

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedComplaintDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items.Select(MapToDto).ToList()
            };
        }

        // ================= UPDATE STATUS (ADMIN) =================
        public async Task<ComplaintResponseDto> UpdateStatusAsync(
            Guid id,
            UpdateComplaintStatusDto dto,
            Guid adminId)
        {
            if (!Enum.TryParse<ComplaintStatus>(dto.Status, true, out var status))
                throw new Exception("Invalid status");

            var repo = _unitOfWork.GetRepository<Complaint>();
            var complaint = await repo.GetByIdAsync(id);

            if (complaint == null || complaint.IsDeleted)
                throw new Exception("Complaint not found");

            // 🔒 Workflow lock
            switch (complaint.Status)
            {
                case ComplaintStatus.Open:
                    if (status != ComplaintStatus.InReview)
                        throw new Exception("Open can only move to InReview");
                    break;

                case ComplaintStatus.InReview:
                    if (status != ComplaintStatus.EnterpriseResponded)
                        throw new Exception("InReview can only move to EnterpriseResponded");
                    break;

                case ComplaintStatus.EnterpriseResponded:
                    if (status != ComplaintStatus.Resolved &&
                        status != ComplaintStatus.Rejected)
                        throw new Exception("EnterpriseResponded can only move to Resolved or Rejected");
                    break;

                default:
                    throw new Exception("Final status cannot be changed");
            }

            complaint.Status = status;
            complaint.LastUpdatedTime = DateTimeOffset.UtcNow;
            complaint.LastUpdatedBy = adminId;

            repo.Update(complaint);
            await _unitOfWork.SaveAsync();
            if (status == ComplaintStatus.Resolved)
            {
                await _citizenPointService.AwardPointsForResolvedComplaintAsync(complaint.Id);
            }
            return MapToDto(complaint);
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(
            Guid id,
            Guid complainantId)
        {
            var repo = _unitOfWork.GetRepository<Complaint>();
            var complaint = await repo.GetByIdAsync(id);

            if (complaint == null || complaint.IsDeleted)
                return false;

            if (complaint.ComplainantId != complainantId)
                throw new Exception("Unauthorized");

            if (complaint.Status != ComplaintStatus.Open)
                throw new Exception("Only Open complaint can be deleted");

            complaint.IsDeleted = true;
            complaint.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(complaint);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= FILTER =================
        private static IQueryable<Complaint> ApplyFilter(
            IQueryable<Complaint> query,
            ComplaintFilterDto filter)
        {
            if (!string.IsNullOrEmpty(filter.Status) &&
                Enum.TryParse<ComplaintStatus>(filter.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(filter.Type) &&
                Enum.TryParse<ComplaintType>(filter.Type, true, out var type))
            {
                query = query.Where(x => x.Type == type);
            }

            if (filter.ReportId.HasValue)
                query = query.Where(x => x.ReportId == filter.ReportId);

            return query;
        }

        // ================= MAPPER =================
        private static ComplaintResponseDto MapToDto(Complaint complaint)
        {
            return new ComplaintResponseDto
            {
                Id = complaint.Id,
                ComplainantId = complaint.ComplainantId,
                ReportId = complaint.ReportId,
                CollectionRequestId = complaint.CollectionRequestId,
                Type = complaint.Type.ToString(),
                Status = complaint.Status.ToString(),
                Content = complaint.Content,
                CreatedTime = complaint.CreatedTime,
                Resolutions = complaint.Resolutions?
                    .Select(r => new DisputeResolutionResponseDto
                    {
                        Id = r.Id,
                        ComplaintId = r.ComplaintId,
                        EnterpriseId = r.EnterpriseId,
                        ResponseNote = r.ResponseNote,
                        ResolvedAt = r.ResolvedAt,
                        CreatedTime = r.CreatedTime
                    }).ToList()
                    ?? new List<DisputeResolutionResponseDto>()
            };
        }
    }
}