using Application.Contract.DTOs;
using Application.Contract.Interfaces;
using Application.Contract.Interfaces.Infrastructure;
using Application.Contract.Interfaces.Services;
using Core.Enum;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class WasteReportService : IWasteReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WasteReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= CREATE =================
        public async Task<WasteReportResponseDto> CreateAsync(
            CreateWasteReportDto dto,
            Guid citizenId)
        {
            if (!dto.Latitude.HasValue || !dto.Longitude.HasValue)
                throw new Exception("Location is required");

            var reportRepo = _unitOfWork.GetRepository<WasteReport>();

            var report = new WasteReport
            {
                CitizenId = citizenId,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = WasteReportStatus.Pending
            };

            await reportRepo.InsertAsync(report);
            await _unitOfWork.SaveAsync();

            return MapToDto(report);
        }

        // ================= UPDATE =================
        public async Task<WasteReportResponseDto> UpdateAsync(
            Guid id,
            UpdateWasteReportDto dto)
        {
            var reportRepo = _unitOfWork.GetRepository<WasteReport>();

            var report = await reportRepo.GetByIdAsync(id);

            if (report == null || report.IsDeleted)
                throw new Exception("WasteReport not found");

            // Không cho sửa khi đã thu gom hoặc bị từ chối
            if (report.Status == WasteReportStatus.Collected ||
                report.Status == WasteReportStatus.Rejected)
            {
                throw new Exception("This report cannot be modified");
            }

            if (!string.IsNullOrWhiteSpace(dto.Description))
                report.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<WasteReportStatus>(dto.Status, true, out var newStatus))
            {
                report.Status = newStatus;
            }

            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            reportRepo.Update(report);
            await _unitOfWork.SaveAsync();

            return MapToDto(report);
        }

        // ================= GET BY ID =================
        public async Task<WasteReportResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.NoTrackingEntities
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return report == null ? null : MapToDto(report);
        }

        // ================= PAGING =================
        public async Task<PagedWasteReportDto> GetPagedAsync(
            WasteReportFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var query = repo.NoTrackingEntities
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(filter.Status) &&
                Enum.TryParse<WasteReportStatus>(filter.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            var pagedData = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedWasteReportDto
            {
                TotalCount = pagedData.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = pagedData.Items
                    .Select(MapToDto)
                    .ToList()
            };
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(id);

            if (report == null)
                return false;

            // Không cho xóa nếu đã thu gom
            if (report.Status == WasteReportStatus.Collected)
                throw new Exception("Collected report cannot be deleted");

            report.IsDeleted = true;
            report.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= MAPPER =================
        private static WasteReportResponseDto MapToDto(WasteReport report)
        {
            return new WasteReportResponseDto
            {
                Id = report.Id,
                CitizenId = report.CitizenId,
                Description = report.Description,
                Latitude = report.Latitude,
                Longitude = report.Longitude,
                Status = report.Status.ToString(),
                CreatedTime = report.CreatedTime
            };
        }
    }
}