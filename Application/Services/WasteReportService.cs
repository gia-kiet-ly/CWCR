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
            var wasteItemRepo = _unitOfWork.GetRepository<WasteReportWaste>();
            var imageRepo = _unitOfWork.GetRepository<WasteImage>();

            // 1. Tạo WasteReport
            var report = new WasteReport
            {
                CitizenId = citizenId,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = WasteReportStatus.Pending
            };

            await reportRepo.InsertAsync(report);
            await _unitOfWork.SaveAsync(); // cần save để report.Id có giá trị

            // 2. Tạo WasteReportWaste + WasteImage
            foreach (var wasteDto in dto.Wastes)
            {
                var wasteItem = new WasteReportWaste
                {
                    WasteReportId = report.Id,
                    WasteTypeId = wasteDto.WasteTypeId,
                    Note = wasteDto.Note
                };

                await wasteItemRepo.InsertAsync(wasteItem);
                await _unitOfWork.SaveAsync(); // cần save để wasteItem.Id có giá trị

                // ← SỬA: lưu từng image URL vào WasteImage
                if (wasteDto.Images != null && wasteDto.Images.Count > 0)
                {
                    foreach (var imageUrl in wasteDto.Images)
                    {
                        if (string.IsNullOrWhiteSpace(imageUrl)) continue;

                        await imageRepo.InsertAsync(new WasteImage
                        {
                            WasteReportWasteId = wasteItem.Id,
                            ImageUrl = imageUrl,
                            PublicId = ExtractPublicId(imageUrl)
                        });
                    }

                    await _unitOfWork.SaveAsync();
                }
            }

            return await GetByIdAsync(report.Id)
                   ?? throw new Exception("Create failed");
        }

        // ================= UPDATE =================
        public async Task<WasteReportResponseDto> UpdateAsync(
            Guid id,
            UpdateWasteReportDto dto)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(id);

            if (report == null || report.IsDeleted)
                throw new Exception("WasteReport not found");

            if (report.Status == WasteReportStatus.Collected ||
                report.Status == WasteReportStatus.Rejected)
                throw new Exception("This report cannot be modified");

            if (!string.IsNullOrWhiteSpace(dto.Description))
                report.Description = dto.Description;

            if (dto.Latitude.HasValue)
                report.Latitude = dto.Latitude;

            if (dto.Longitude.HasValue)
                report.Longitude = dto.Longitude;

            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse<WasteReportStatus>(dto.Status, true, out var newStatus))
            {
                report.Status = newStatus;
            }

            report.LastUpdatedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            return await GetByIdAsync(id)
                   ?? throw new Exception("Update failed");
        }

        // ================= GET BY ID =================
        public async Task<WasteReportResponseDto?> GetByIdAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.NoTrackingEntities
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.WasteType)
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            return report == null ? null : MapToDto(report);
        }

        // ================= GET PAGED =================
        public async Task<PagedWasteReportDto> GetPagedAsync(
            WasteReportFilterDto filter)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var query = repo.NoTrackingEntities
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.WasteType)
                .Include(x => x.Wastes)
                    .ThenInclude(x => x.Images)
                .Where(x => !x.IsDeleted);

            if (filter.CitizenId.HasValue)
                query = query.Where(x => x.CitizenId == filter.CitizenId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                Enum.TryParse<WasteReportStatus>(filter.Status, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.ToLower();
                query = query.Where(x =>
                    x.Description != null &&
                    x.Description.ToLower().Contains(keyword));
            }

            var paged = await repo.GetPagging(
                query.OrderByDescending(x => x.CreatedTime),
                filter.PageNumber,
                filter.PageSize);

            return new PagedWasteReportDto
            {
                TotalCount = paged.TotalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                Items = paged.Items.Select(MapToDto).ToList()
            };
        }

        // ================= DELETE (SOFT) =================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var repo = _unitOfWork.GetRepository<WasteReport>();

            var report = await repo.GetByIdAsync(id);

            if (report == null || report.IsDeleted)
                return false;

            if (report.Status == WasteReportStatus.Collected)
                throw new Exception("Collected report cannot be deleted");

            report.IsDeleted = true;
            report.DeletedTime = DateTimeOffset.UtcNow;

            repo.Update(report);
            await _unitOfWork.SaveAsync();

            return true;
        }

        // ================= HELPERS =================

        /// <summary>
        /// Lấy publicId từ Cloudinary URL.
        /// VD: https://res.cloudinary.com/demo/image/upload/v123/waste_images/abc.png → waste_images/abc
        /// </summary>
        private static string ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/');
                // Tìm index "upload", lấy từ sau version (v...) đến hết, bỏ extension
                var uploadIdx = Array.IndexOf(segments, "upload");
                if (uploadIdx < 0) return url;

                // Bỏ segment version (bắt đầu bằng 'v' + số)
                var afterUpload = segments.Skip(uploadIdx + 1)
                    .SkipWhile(s => s.Length > 1 && s[0] == 'v' && s.Skip(1).All(char.IsDigit))
                    .ToArray();

                var joined = string.Join("/", afterUpload);
                var dotIdx = joined.LastIndexOf('.');
                return dotIdx > 0 ? joined[..dotIdx] : joined;
            }
            catch
            {
                return url;
            }
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
                CreatedTime = report.CreatedTime,

                Wastes = report.Wastes.Select(x => new WasteItemResponseDto
                {
                    WasteTypeId = x.WasteTypeId,
                    WasteTypeName = x.WasteType?.Name,
                    Note = x.Note,
                    ImageUrls = x.Images
                        .Where(i => !i.IsDeleted)
                        .Select(i => i.ImageUrl)
                        .ToList()
                }).ToList()
            };
        }
    }
}